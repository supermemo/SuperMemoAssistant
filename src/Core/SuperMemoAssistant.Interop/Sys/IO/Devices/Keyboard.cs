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
// Created On:   2018/05/31 03:56
// Modified On:  2018/05/31 04:00
// Modified By:  Alexis

#endregion




using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Sys.IO.Devices
{
  public static class Keyboard
  {
    #region Methods

    public static bool GetKeyState(VKey vkey)
    {
      return (Native.GetKeyState((int)vkey) & 0xF0) == 1;
    }

    public static async Task<bool> SendSysKeysAsync(IntPtr hWnd, Keys keys, int delay = 10)
    {
      bool result = true;

      SendKeyDown(hWnd, VKey.KEY_MENU, true);

      foreach (var (vKey, task) in keys.VKeys)
      {
        SendKeyPress(hWnd, vKey, true);

        if (task != null)
          if (await task.ConfigureAwait(false) == false)
          {
            result = false;
            break;
          }
      }

      SendKeyUp(hWnd, VKey.KEY_MENU);

      return result;
    }

    public static async Task<bool> SendKeysAsync(IntPtr hWnd, Keys keys, int delay = 10)
    {
      bool result = true;

      foreach (VKey vkey in keys.VKeysModifiers)
        SendKeyDown(hWnd, vkey);

      foreach (var (vKey, task) in keys.VKeys)
      {
        SendKeyPress(hWnd, vKey);

        if (task != null)
          if (await task.ConfigureAwait(false) == false)
          {
            result = false;
            break;
          }
      }

      foreach (VKey vkey in keys.VKeysModifiers)
        SendKeyUp(hWnd, vkey);

      return result;
    }

    public static async Task<bool> PostSysKeysAsync(IntPtr hWnd, Keys keys, int delay = 10)
    {
      bool result = true;

      PostKeyDown(hWnd, VKey.KEY_MENU, true);

      foreach (var (vKey, task) in keys.VKeys)
      {
        PostKeyPress(hWnd, vKey, true);

        if (task != null)
          if (await task.ConfigureAwait(false) == false)
          {
            result = false;
            break;
          }
      }

      PostKeyUp(hWnd, VKey.KEY_MENU);

      return result;
#if false
    PostMessage(hWnd, (int)Message.SYSKEY_DOWN, (int)VKey.KEY_MENU, 0x3038001);
    PostMessage(hWnd, (int)Message.SYSKEY_DOWN, (int)VKey.KEY_F, 0x70210001);
    
    PostMessage(hWnd, (int)Message.SYSKEY_UP, (int)VKey.KEY_F, 0xF0210001);
    PostMessage(hWnd, (int)Message.KEY_UP, (int)VKey.KEY_MENU, 0xD0380001);
#endif
    }

    public static async Task<bool> PostKeysAsync(IntPtr hWnd, Keys keys, int delay = 10)
    {
      bool result = true;

      foreach (VKey vkey in keys.VKeysModifiers)
        PostKeyDown(hWnd, vkey);

      foreach (var (vKey, task) in keys.VKeys)
      {
        PostKeyPress(hWnd, vKey);

        if (task != null)
          if (await task.ConfigureAwait(false) == false)
          {
            result = false;
            break;
          }
      }

      foreach (VKey vkey in keys.VKeysModifiers)
        PostKeyUp(hWnd, vkey);

      return result;
    }

    public static bool SendKeyPress(IntPtr hWnd, VKey vkey, bool sys = false, int delay = 10)
    {
      if (!SendKeyDown(hWnd, vkey, sys))
        return false;

      //Send VM_CHAR
      //if (SendMessage(hWnd, (int)Message.VM_CHAR, (uint)key, GetLParam(1, key, 0, 0, 0, 0)))
      //  return false;

      Thread.Sleep(delay);

      return SendKeyUp(hWnd, vkey, sys);
    }

    public static bool PostKeyPress(IntPtr hWnd, VKey vkey, bool sys = false, int delay = 10)
    {
      if (!PostKeyDown(hWnd, vkey, sys))
        return false;

      //Send VM_CHAR
      //if (PostMessage(hWnd, (int)Message.VM_CHAR, (uint)key, GetLParam(1, key, 0, 0, 0, 0)))
      //  return false;

      Thread.Sleep(delay);

      return PostKeyUp(hWnd, vkey, sys);
    }

    public static bool SendKeyDown(IntPtr hWnd, VKey vkey, bool sys = false)
    {
      return Native.SendMessage(
        hWnd,
        (int)(sys ? Message.SYSKEY_DOWN : Message.KEY_DOWN),
        (uint)vkey, Native.GetLParam(1, vkey, 0, (byte)(sys ? 1 : 0), 0, 0)
      ) == 0;
    }

    public static bool SendKeyUp(IntPtr hWnd, VKey vkey, bool sys = false)
    {
      return Native.SendMessage(
        hWnd,
        (int)(sys ? Message.SYSKEY_UP : Message.KEY_UP),
        (uint)vkey, Native.GetLParam(1, vkey, 0, (byte)(sys ? 1 : 0), 1, 1)
      ) == 0;
    }

    public static bool PostKeyDown(IntPtr hWnd, VKey vkey, bool sys = false)
    {
      return Native.PostMessage(
        hWnd,
        (int)(sys ? Message.SYSKEY_DOWN : Message.KEY_DOWN),
        (uint)vkey, Native.GetLParam(1, vkey, 0, (byte)(sys ? 1 : 0), 0, 0)
      );
    }

    public static bool PostKeyUp(IntPtr hWnd, VKey vkey, bool sys = false)
    {
      return Native.PostMessage(
        hWnd,
        (int)(sys ? Message.SYSKEY_UP : Message.KEY_UP),
        (uint)vkey, Native.GetLParam(1, vkey, 0, (byte)(sys ? 1 : 0), 1, 1)
      );
    }

    #endregion




    //public static void CheckKeyShiftState()
    //{
    //  while ((GetKeyState((int)VKeys.KEY_MENU) & KEY_PRESSED) == KEY_PRESSED ||
    //         (GetKeyState((int)VKeys.KEY_CONTROL) & KEY_PRESSED) == KEY_PRESSED ||
    //         (GetKeyState((int)VKeys.KEY_SHIFT) & KEY_PRESSED) == KEY_PRESSED)
    //  {
    //    Thread.Sleep(1);
    //  }
    //}
  }
}
