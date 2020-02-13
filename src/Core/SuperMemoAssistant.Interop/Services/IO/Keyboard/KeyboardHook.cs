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
// Created On:   2020/01/13 16:38
// Modified On:  2020/01/22 16:47
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Sys.IO.Devices;
using SuperMemoAssistant.Sys.Remoting;

// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.Services.IO.Keyboard
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
    private bool     _isDisposed;
    private IntPtr   _windowsHookHandle;
    private IntPtr   _elWdwHandle;
    private int      _smProcessId;

    private ConcurrentDictionary<HotKey, RegisteredHotKey> HotKeys { get; } =
      new ConcurrentDictionary<HotKey, RegisteredHotKey>();
    private ConcurrentQueue<Action> TriggeredCallbacks { get; } = new ConcurrentQueue<Action>();
    private AutoResetEvent          TriggeredEvent     { get; } = new AutoResetEvent(false);

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

      Svc.OnSMAAvailable += OnSMAAvailable;
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

    public Action<HotKey> MainCallback { get; set; }

    #endregion




    #region Methods Impl

    public bool UnregisterHotKey(HotKey hotkey)
    {
      return HotKeys.TryRemove(hotkey,
                               out _);
    }

    public void RegisterHotKey(HotKey      hotkey,
                               Action      callback,
                               HotKeyScope scope = HotKeyScope.SM)
    {
      HotKeys[hotkey] = new RegisteredHotKey(callback, scope);
    }

    #endregion




    #region Methods

    private void ExecuteCallbacks()
    {
      while (_isDisposed == false)
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

    [LogToErrorOnException]
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
          var hk = new HotKey(
            kbEvent.Key,
            GetCtrlPressed(), GetAltPressed(), GetShiftPressed(), GetMetaPressed());
          var hkReg = HotKeys.SafeGet(hk);

          if (MainCallback != null && hk.Modifiers != KeyModifiers.None)
            MainCallback(hk);

          if (hkReg != null)
          {
            bool scopeMatches = true;

            if (hkReg.Scope != HotKeyScope.Global)
            {
              var foregroundWdwHandle = GetForegroundWindow();

              // ReSharper disable once ConditionIsAlwaysTrueOrFalse
              if (foregroundWdwHandle == null || foregroundWdwHandle == IntPtr.Zero)
              {
                scopeMatches = false;
              }
              
              // ReSharper disable once ConditionIsAlwaysTrueOrFalse
              else if (_elWdwHandle == null || _elWdwHandle == IntPtr.Zero)
              {
                LogTo.Warning(
                  $"KeyboardHook: HotKey {hk} requested with scope {Enum.GetName(typeof(HotKeyScope), hkReg.Scope)}, but _elWdwHandle is {_elWdwHandle}. Trying to refresh.");

                OnElementWindowAvailable();
                
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (_elWdwHandle == null || _elWdwHandle == IntPtr.Zero)
                  scopeMatches = false;
              }

              else if (hkReg.Scope == HotKeyScope.SMBrowser && foregroundWdwHandle != _elWdwHandle)
              {
                scopeMatches = false;
              }

              else if (hkReg.Scope == HotKeyScope.SM)
              {
                GetWindowThreadProcessId(foregroundWdwHandle, out var foregroundProcId);

                if (foregroundProcId != _smProcessId)
                  scopeMatches = false;
              }
            }

            if (scopeMatches)
            {
              TriggeredCallbacks.Enqueue(hkReg.Callback);
              TriggeredEvent.Set();

              return (IntPtr)1;
            }
          }
        }
      }

      return CallNextHookEx(_windowsHookHandle,
                            nCode,
                            wParam,
                            lParam);
    }

    private void OnSMAAvailable(Interop.SuperMemo.ISuperMemoAssistant sma)
    {
      if (sma.SM.UI.ElementWdw.IsAvailable)
        OnElementWindowAvailable();

      else
        sma.SM.UI.ElementWdw.OnAvailable += new ActionProxy(OnElementWindowAvailable);
    }

    private void OnElementWindowAvailable()
    {
      _elWdwHandle = Svc.SM.UI.ElementWdw.Handle;
      _smProcessId = Svc.SM.ProcessId;
    }

    #endregion




    #region Events

    public event EventHandler<KeyboardHookEventArgs> KeyboardPressed;

    #endregion




    private class RegisteredHotKey
    {
      #region Constructors

      public RegisteredHotKey(Action callback, HotKeyScope scope)
      {
        Callback = callback;
        Scope    = scope;
      }

      #endregion




      #region Properties & Fields - Public

      public Action      Callback { get; }
      public HotKeyScope Scope    { get; }

      #endregion
    }
  }

  [Flags]
  public enum HotKeyScope
  {
    SMBrowser = 1,
    SM        = 0xFFFF,

    Global = 0xFFFFFFF
  }
}
