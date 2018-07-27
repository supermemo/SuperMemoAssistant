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
// Created On:   2018/05/31 03:57
// Modified On:  2018/05/31 04:00
// Modified By:  Alexis

#endregion




using System;
using System.Threading;

namespace SuperMemoAssistant.Sys.IO.Devices {
  internal static class Mouse
  {
    #region Methods

    public static void MousePosition(IntPtr hWnd, int x, int y)
    {
      Native.PostMessage(hWnd, (int)Message.MOUSEMOVE, 0, Native.GetLParam(x, y));
    }

    public static void MouseClick(IntPtr hWnd, VKey vkey, int x, int y, int delay = 10)
    {
      switch (vkey)
      {
        case VKey.KEY_MBUTTON:
          Native.SendMessage(hWnd, (int)Message.MBUTTONDOWN, (uint)vkey, Native.GetLParam(x, y));
          Thread.Sleep(delay);
          Native.SendMessage(hWnd, (int)Message.MBUTTONUP, (uint)vkey, Native.GetLParam(x, y));
          break;

        case VKey.KEY_LBUTTON:
          Native.SendMessage(hWnd, (int)Message.LBUTTONDOWN, (uint)vkey, Native.GetLParam(x, y));
          Thread.Sleep(delay);
          Native.SendMessage(hWnd, (int)Message.LBUTTONUP, (uint)vkey, Native.GetLParam(x, y));
          break;

        case VKey.KEY_RBUTTON:
          Native.SendMessage(hWnd, (int)Message.RBUTTONDOWN, (uint)vkey, Native.GetLParam(x, y));
          Thread.Sleep(delay);
          Native.SendMessage(hWnd, (int)Message.RBUTTONUP, (uint)vkey, Native.GetLParam(x, y));
          break;
      }
    }

    #endregion
  }
}
