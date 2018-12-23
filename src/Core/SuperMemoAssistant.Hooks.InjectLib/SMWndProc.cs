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
// Created On:   2018/12/20 02:47
// Modified On:  2018/12/20 02:50
// Modified By:  Alexis

#endregion




using System;
using Process.NET.Native.Types;
using Process.NET.Windows;

namespace SuperMemoAssistant.Hooks.InjectLib
{
  public class SMWndProc : WindowProcHook
  {
    #region Constructors

    /// <inheritdoc />
    public SMWndProc()
      : base(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle) { }

    protected override IntPtr OnWndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
    {

      try
      {
        if (msg == (int)WindowsMessages.User && HandleUserMessage(wParam.ToInt32()))
          return IntPtr.Zero;

        return base.OnWndProc(hWnd, msg, wParam, lParam);
      }
      catch (Exception)
      {
        return IntPtr.Zero;
      }
    }

    private bool HandleUserMessage(int wParam)
    {
      return SMInject.Instance.OnUserMessage(wParam);
    }

    #endregion
  }
}
