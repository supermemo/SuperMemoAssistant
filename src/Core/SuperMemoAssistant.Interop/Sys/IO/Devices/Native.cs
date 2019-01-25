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
// Created On:   2018/06/01 14:25
// Modified On:  2019/01/25 23:40
// Modified By:  Alexis

#endregion




using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.Sys.IO.Devices
{
  /// <summary>Class for messaging and key presses</summary>
  public static class Native
  {
    #region Constants & Statics

    public const int WH_KEYBOARD_LL = 13;

    /// <summary>Maps a virtual key to a key code with specified keyboard.</summary>
    public const uint MAPVK_VK_TO_VSC_EX = 0x04;

    /// <summary>Code for if the key is pressed.</summary>
    public const ushort KEY_PRESSED = 0xF000;
    private const uint Lower16BitsMask = 0xFFFF;

    #endregion




    #region Methods

    [DllImport("user32.dll")]
    public static extern ushort GetKeyState(int nVirtKey);

    /// <summary>Gets the state of the entire keyboard.</summary>
    /// <param name="lpKeyState">The byte array to receive all the keys states.</param>
    /// <returns>Whether it succeed or failed.</returns>
    [DllImport("user32.dll")]
    public static extern bool GetKeyboardState(byte[] lpKeyState);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    //[return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SendMessage(IntPtr hWnd,
                                         int    wMsg,
                                         uint   wParam,
                                         uint   lParam);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool PostMessage(IntPtr hWnd,
                                          int    Msg,
                                          uint   wParam,
                                          uint   lParam);

    [DllImport("user32.dll")]
    public static extern uint MapVirtualKey(uint uCode,
                                            uint uMapType);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetKeyboardState([In] byte[] keyboardState);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RegisterHotKey(IntPtr hWnd,
                                             int    id,
                                             uint   fsModifiers,
                                             uint   vk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnregisterHotKey(IntPtr hWnd,
                                               int    id);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int SetWindowsHookEx(int                 idHook,
                                              KeyboardHookHandler lpfn,
                                              IntPtr              hMod,
                                              uint                dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(int hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int CallNextHookEx(int             hhk,
                                            int             nCode,
                                            int             wParam,
                                            KBDLLHOOKSTRUCT lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);


    public static uint GetScanCode(VKey vkey)
    {
      //uint scanCode = MapVirtualKey((uint)key, MAPVK_VK_TO_CHAR);
      return MapVirtualKey((uint)vkey, MAPVK_VK_TO_VSC_EX);
    }

    public static uint GetLParam(int x,
                                 int y)
    {
      return (uint)((y << 16) | (x & 0xFFFF));
    }

    public static uint GetLParam(Int16 repeatCount,
                                 VKey  vkey,
                                 byte  extended,
                                 byte  contextCode,
                                 byte  previousState,
                                 byte  transitionState)
    {
      var  lParam   = (uint)repeatCount;
      uint scanCode = GetScanCode(vkey);

      lParam += scanCode * 0x10000;
      lParam += (uint)(extended * 0x1000000);
      lParam += (uint)(contextCode * 2 * 0x10000000);
      lParam += (uint)(previousState * 4 * 0x10000000);
      lParam += (uint)(transitionState * 8 * 0x10000000);

      return lParam;
    }

    #endregion




    public delegate int KeyboardHookHandler(int             nCode,
                                            int             wParam,
                                            KBDLLHOOKSTRUCT lParam);
  }

  [StructLayout(LayoutKind.Sequential)]
  public class KBDLLHOOKSTRUCT
  {
    #region Properties & Fields - Public

    public UIntPtr   dwExtraInfo;
    public HookFlags flags;
    public uint      scanCode;
    public uint      time;
    public VKey      vkCode;

    #endregion
  }

  [Flags]
  public enum HookFlags : uint
  {
    LLKHF_EXTENDED = 0x01,
    LLKHF_INJECTED = 0x10,
    LLKHF_ALTDOWN  = 0x20,
    LLKHF_UP       = 0x80,
  }

  public enum Message
  {
    KEY_DOWN      = 0x100, //Key down
    KEY_UP        = 0x101, //Key Up
    CHAR          = 0x102, //The character being pressed
    SYSKEY_DOWN   = 0x104, //An Alt/ctrl/shift + key down message
    SYSKEY_UP     = 0x105, //An Alt/Ctrl/Shift + Key up Message
    SYSCHAR       = 0x106, //An Alt/Ctrl/Shift + Key character Message
    MOUSEMOVE     = 0x200,
    LBUTTONDOWN   = 0x201, //Left mousebutton down 
    LBUTTONUP     = 0x202, //Left mousebutton up 
    LBUTTONDBLCLK = 0x203, //Left mousebutton doubleclick 
    RBUTTONDOWN   = 0x204, //Right mousebutton down 
    RBUTTONUP     = 0x205, //Right mousebutton up 
    RBUTTONDBLCLK = 0x206, //Right mousebutton doubleclick

    /// <summary>Middle mouse button down</summary>
    MBUTTONDOWN = 0x207,

    /// <summary>Middle mouse button up</summary>
    MBUTTONUP = 0x208,

    HOTKEY = 0x312,
  }
}
