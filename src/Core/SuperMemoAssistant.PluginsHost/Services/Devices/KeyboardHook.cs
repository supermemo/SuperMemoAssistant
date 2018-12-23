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
// Created On:   2018/06/01 14:29
// Modified On:  2018/12/21 02:24
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Services.IO.Devices;
using SuperMemoAssistant.Sys.IO.Devices;

// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.PluginsHost.Services.Devices
{
  // https://stackoverflow.com/questions/604410/global-keyboard-capture-in-c-sharp-application

  //Based on https://gist.github.com/Stasonix
  public class KeyboardHookService : IDisposable, IKeyboardHookService
  {
    #region Constants & Statics

    public const int WH_KEYBOARD_LL = 13;


    public static KeyboardHookService Instance { get; } = new KeyboardHookService();

    #endregion




    #region Properties & Fields - Non-Public

    private HookProc _hookProc;
    private IntPtr   _user32LibraryHandle;

    private IntPtr _windowsHookHandle;

    #endregion




    #region Constructors

    protected KeyboardHookService()
    {
      _windowsHookHandle   = IntPtr.Zero;
      _user32LibraryHandle = IntPtr.Zero;
      _hookProc =
        LowLevelKeyboardProc; // we must keep alive _hookProc, because GC is not aware about SetWindowsHookEx behaviour.

      _user32LibraryHandle = LoadLibrary("User32");
      if (_user32LibraryHandle == IntPtr.Zero)
      {
        int errorCode = Marshal.GetLastWin32Error();
        throw new Win32Exception(errorCode,
                                 $"Failed to load library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
      }


      _windowsHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL,
                                            _hookProc,
                                            _user32LibraryHandle,
                                            0);
      if (_windowsHookHandle == IntPtr.Zero)
      {
        int errorCode = Marshal.GetLastWin32Error();
        throw new Win32Exception(errorCode,
                                 $"Failed to adjust keyboard hooks for '{System.Diagnostics.Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion




    #region Properties & Fields - Public

    public ConcurrentDictionary<HotKey, Func<bool>> HotKeys { get; } = new ConcurrentDictionary<HotKey, Func<bool>>();

    #endregion




    #region Methods Impl

    ~KeyboardHookService()
    {
      Dispose(false);
    }

    public void RegisterHotKey(HotKey     hotkey,
                               Func<bool> callback)
    {
      HotKeys[hotkey] = callback;
    }

    public bool UnregisterHotKey(HotKey hotkey)
    {
      return HotKeys.TryRemove(hotkey,
                               out var _);
    }

    #endregion




    #region Methods

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
        if (_windowsHookHandle != IntPtr.Zero)
        {
          if (!UnhookWindowsHookEx(_windowsHookHandle))
          {
            int errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode,
                                     $"Failed to remove keyboard hooks for '{System.Diagnostics.Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
          }

          _windowsHookHandle = IntPtr.Zero;

          // ReSharper disable once DelegateSubtraction
          _hookProc -= LowLevelKeyboardProc;
        }

      if (_user32LibraryHandle != IntPtr.Zero)
      {
        if (!FreeLibrary(_user32LibraryHandle)) // reduces reference to library by 1.
        {
          int errorCode = Marshal.GetLastWin32Error();
          throw new Win32Exception(errorCode,
                                   $"Failed to unload library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
        }

        _user32LibraryHandle = IntPtr.Zero;
      }
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll",
      CharSet = CharSet.Auto)]
    private static extern bool FreeLibrary(IntPtr hModule);

    /// <summary>
    ///   The SetWindowsHookEx function installs an application-defined hook procedure into a
    ///   hook chain. You would install a hook procedure to monitor the system for certain types of
    ///   events. These events are associated either with a specific thread or with all threads in the
    ///   same desktop as the calling thread.
    /// </summary>
    /// <param name="idHook">hook type</param>
    /// <param name="lpfn">hook procedure</param>
    /// <param name="hMod">handle to application instance</param>
    /// <param name="dwThreadId">thread identifier</param>
    /// <returns>If the function succeeds, the return value is the handle to the hook procedure.</returns>
    [DllImport("USER32",
      SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int      idHook,
                                                  HookProc lpfn,
                                                  IntPtr   hMod,
                                                  int      dwThreadId);

    /// <summary>
    ///   The UnhookWindowsHookEx function removes a hook procedure installed in a hook chain
    ///   by the SetWindowsHookEx function.
    /// </summary>
    /// <param name="hHook">handle to hook procedure</param>
    /// <returns>If the function succeeds, the return value is true.</returns>
    [DllImport("USER32",
      SetLastError = true)]
    public static extern bool UnhookWindowsHookEx(IntPtr hHook);

    /// <summary>
    ///   The CallNextHookEx function passes the hook information to the next hook procedure in
    ///   the current hook chain. A hook procedure can call this function either before or after
    ///   processing the hook information.
    /// </summary>
    /// <param name="hHook">handle to current hook</param>
    /// <param name="code">hook code passed to hook procedure</param>
    /// <param name="wParam">value passed to hook procedure</param>
    /// <param name="lParam">value passed to hook procedure</param>
    /// <returns>If the function succeeds, the return value is true.</returns>
    [DllImport("USER32",
      SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hHook,
                                                int    code,
                                                IntPtr wParam,
                                                IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern short GetKeyState(System.Windows.Forms.Keys nVirtKey);

    private static bool GetCapslock()
    {
      return Convert.ToBoolean(GetKeyState(System.Windows.Forms.Keys.CapsLock)) & true;
    }

    private static bool GetNumlock()
    {
      return Convert.ToBoolean(GetKeyState(System.Windows.Forms.Keys.NumLock)) & true;
    }

    private static bool GetScrollLock()
    {
      return Convert.ToBoolean(GetKeyState(System.Windows.Forms.Keys.Scroll)) & true;
    }

    private static bool GetCtrlPressed()
    {
      int state = GetKeyState(System.Windows.Forms.Keys.ControlKey);
      if (state > 1 || state < -1) return true;

      return false;
    }

    private static bool GetAltPressed()
    {
      int state = GetKeyState(System.Windows.Forms.Keys.Menu);
      if (state > 1 || state < -1) return true;

      return false;
    }

    private static bool GetShiftPressed()
    {
      int state = GetKeyState(System.Windows.Forms.Keys.ShiftKey);
      if (state > 1 || state < -1) return true;

      return false;
    }

    private static bool GetMetaPressed()
    {
      int state = GetKeyState(System.Windows.Forms.Keys.LWin);
      if (state > 1 || state < -1) return true;

      state = GetKeyState(System.Windows.Forms.Keys.RWin);
      if (state > 1 || state < -1) return true;

      return false;
    }

    private IntPtr LowLevelKeyboardProc(int    nCode,
                                        IntPtr wParam,
                                        IntPtr lParam)
    {
      if (nCode < 0)
        return CallNextHookEx(IntPtr.Zero,
                              nCode,
                              wParam,
                              lParam);

      bool fEatKeyStroke = false;

      var wparamTyped = wParam.ToInt32();

      if (Enum.IsDefined(typeof(KeyboardState),
                         wparamTyped))
      {
        object o = Marshal.PtrToStructure(lParam,
                                          typeof(LowLevelKeyboardInputEvent));
        LowLevelKeyboardInputEvent p = (LowLevelKeyboardInputEvent)o;

        var eArgs = new KeyboardHookEventArgs(p,
                                              (KeyboardState)wparamTyped,
                                              GetCtrlPressed(),
                                              GetAltPressed(),
                                              GetShiftPressed(),
                                              GetMetaPressed());

        //EventHandler<KeyboardHookEventArgs> handler = KeyboardPressed;
        //handler?.Invoke(this,
                        //eArgs);

        fEatKeyStroke = eArgs.Handled;

        if (fEatKeyStroke == false && eArgs.KeyboardState == KeyboardState.KeyDown || eArgs.KeyboardState == KeyboardState.SysKeyDown)
        {
          var func = HotKeys.SafeGet(new HotKey(eArgs.Ctrl,
                                                eArgs.Alt,
                                                eArgs.Shift,
                                                eArgs.Meta,
                                                eArgs.KeyboardData.Key,
                                                null)
          );

          if (func != null)
          {
            new Thread(() => func.Invoke()).Start();
            fEatKeyStroke = true;
          }
        }
      }

      return fEatKeyStroke
        ? (IntPtr)1
        : CallNextHookEx(IntPtr.Zero,
                         nCode,
                         wParam,
                         lParam);
    }

    #endregion




    #region Events

    public event EventHandler<KeyboardHookEventArgs> KeyboardPressed;

    #endregion




    private delegate IntPtr HookProc(int    nCode,
                                     IntPtr wParam,
                                     IntPtr lParam);
  }
}
