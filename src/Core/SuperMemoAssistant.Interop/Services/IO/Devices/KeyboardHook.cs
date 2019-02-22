#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   2019/02/21 19:06
// Modified On:  2019/02/22 13:03
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Sys.IO.Devices;

// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.Services.IO.Devices
{
  // https://stackoverflow.com/questions/604410/global-keyboard-capture-in-c-sharp-application
  // Based on https://gist.github.com/Stasonix
  public partial class KeyboardHookService : IDisposable, IKeyboardHookService
  {
    #region Constants & Statics

    public static KeyboardHookService Instance { get; } = new KeyboardHookService();

    #endregion




    #region Properties & Fields - Non-Public

    private HookProc _hookProc;
    private IntPtr   _windowsHookHandle;
    private bool _isDisposed;

    #endregion




    #region Constructors

    protected KeyboardHookService()
    {
      // we must keep alive _hookProc, because GC is not aware about SetWindowsHookEx behaviour.
      _hookProc          = LowLevelKeyboardProc;
      _windowsHookHandle = IntPtr.Zero;

      Task.Factory.StartNew(ExecuteCallbacks, TaskCreationOptions.LongRunning);

      using (Process curProcess = Process.GetCurrentProcess())
      using (ProcessModule curModule = curProcess.MainModule)
        _windowsHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL,
                                              _hookProc,
                                              GetModuleHandle(curModule.ModuleName),
                                              0);

      if (_windowsHookHandle == IntPtr.Zero)
      {
        int errorCode = Marshal.GetLastWin32Error();
        throw new Win32Exception(errorCode,
                                 $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
      }
    }

    public void Dispose()
    {
      _isDisposed = true;
      TriggeredEvent.Set();

      if (_windowsHookHandle != IntPtr.Zero)
      {
        if (!UnhookWindowsHookEx(_windowsHookHandle))
        {
          int errorCode = Marshal.GetLastWin32Error();
          throw new Win32Exception(errorCode,
                                   $"Failed to remove keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
        }

        _windowsHookHandle = IntPtr.Zero;

        // ReSharper disable once DelegateSubtraction
        _hookProc -= LowLevelKeyboardProc;
      }

      GC.SuppressFinalize(this);
    }

    #endregion




    #region Properties & Fields - Public

    private ConcurrentDictionary<HotKey, Action> HotKeys { get; } = new ConcurrentDictionary<HotKey, Action>();
    private ConcurrentQueue<Action> TriggeredCallbacks { get; } = new ConcurrentQueue<Action>();
    private AutoResetEvent TriggeredEvent { get; } = new AutoResetEvent(false);

    #endregion




    #region Methods Impl

    public void RegisterHotKey(HotKey hotkey,
                               Action callback)
    {
      HotKeys[hotkey] = callback;
    }

    public bool UnregisterHotKey(HotKey hotkey)
    {
      return HotKeys.TryRemove(hotkey,
                               out _);
    }

    #endregion




    #region Methods

    private void ExecuteCallbacks()
    {
      while (_isDisposed == false)
      {
        try
        {
          TriggeredEvent.WaitOne();

          while (TriggeredCallbacks.TryDequeue(out var callback))
            callback();
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "An exception was thrown while executing Keyboard HotKey callback");
        }
      }
    }

    private IntPtr LowLevelKeyboardProc(int    nCode,
                                        IntPtr wParam,
                                        IntPtr lParam)
    {
      if (nCode < 0)
        return CallNextHookEx(_windowsHookHandle,
                              nCode,
                              wParam,
                              lParam);

      var wparamTyped = wParam.ToInt32();

      // TODO: Invoke KeyboardPressed

      if (Enum.IsDefined(typeof(KeyboardState),
                         wparamTyped))
      {
        KeyboardState kbState = (KeyboardState)wparamTyped;
        LowLevelKeyboardInputEvent kbEvent = (LowLevelKeyboardInputEvent)Marshal.PtrToStructure(lParam,
                                                                                                typeof(LowLevelKeyboardInputEvent));

        if (kbState == KeyboardState.KeyDown || kbState == KeyboardState.SysKeyDown)
        {
          var callback = HotKeys.SafeGet(new HotKey(GetCtrlPressed(),
                                                GetAltPressed(),
                                                GetShiftPressed(),
                                                GetMetaPressed(),
                                                kbEvent.Key,
                                                null)
          );

          if (callback != null)
          {
            TriggeredCallbacks.Enqueue(callback);
            TriggeredEvent.Set();
            
            return (IntPtr)1;
          }
        }
      }

      return CallNextHookEx(_windowsHookHandle,
                            nCode,
                            wParam,
                            lParam);
    }

    #endregion




    #region Events

    public event EventHandler<KeyboardHookEventArgs> KeyboardPressed;

    #endregion
  }

  public enum KeyboardState
  {
    KeyDown    = 0x0100,
    KeyUp      = 0x0101,
    SysKeyDown = 0x0104,
    SysKeyUp   = 0x0105
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct LowLevelKeyboardInputEvent
  {
    /// <summary>A virtual-key code. The code must be a value in the range 1 to 254.</summary>
    public readonly int VirtualCode;

    /// <summary>A hardware scan code for the key.</summary>
    public readonly int HardwareScanCode;

    /// <summary>
    ///   The extended-key flag, event-injected Flags, context code, and transition-state flag.
    ///   This member is specified as follows. An application can use the following values to test the
    ///   keystroke Flags. Testing LLKHF_INJECTED (bit 4) will tell you whether the event was
    ///   injected. If it was, then testing LLKHF_LOWER_IL_INJECTED (bit 1) will tell you whether or
    ///   not the event was injected from a process running at lower integrity level.
    /// </summary>
    public readonly int Flags;

    /// <summary>
    ///   The time stamp stamp for this message, equivalent to what GetMessageTime would return
    ///   for this message.
    /// </summary>
    public readonly int TimeStamp;

    /// <summary>Additional information associated with the message.</summary>
    public readonly IntPtr AdditionalInformation;

    public Key Key => KeyInterop.KeyFromVirtualKey(VirtualCode);
  }
}
