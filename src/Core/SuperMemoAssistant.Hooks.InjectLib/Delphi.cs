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
// Created On:   2019/02/24 23:59
// Modified On:  2019/02/25 00:00
// Modified By:  Alexis

#endregion




using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.Hooks.InjectLib
{
  public static class Delphi
  {
    #region Methods

    [DllImport("SuperMemoAssistant.Hooks.NativeLib.dll",
      CallingConvention = CallingConvention.StdCall)]
    public static extern int registerCall1(
      int functionPtr,
      int arg1);

    [DllImport("SuperMemoAssistant.Hooks.NativeLib.dll",
      CallingConvention = CallingConvention.StdCall)]
    public static extern int registerCall2(
      int functionPtr,
      int arg1,
      int arg2);

    [DllImport("SuperMemoAssistant.Hooks.NativeLib.dll",
      CallingConvention = CallingConvention.StdCall)]
    public static extern int registerCall3(
      int functionPtr,
      int arg1,
      int arg2,
      int arg3);

    [DllImport("SuperMemoAssistant.Hooks.NativeLib.dll",
      CallingConvention = CallingConvention.StdCall)]
    public static extern int registerCall4(
      int functionPtr,
      int arg1,
      int arg2,
      int arg3,
      int arg4);

    [DllImport("SuperMemoAssistant.Hooks.NativeLib.dll",
      CallingConvention = CallingConvention.StdCall)]
    public static extern int registerCall5(
      int functionPtr,
      int arg1,
      int arg2,
      int arg3,
      int arg4,
      int arg5);

    [DllImport("SuperMemoAssistant.Hooks.NativeLib.dll",
      CallingConvention = CallingConvention.StdCall)]
    public static extern int registerCall6(
      int functionPtr,
      int arg1,
      int arg2,
      int arg3,
      int arg4,
      int arg5,
      int arg6);

    #endregion




    public struct TMsg
    {
      public int hwnd;
      public int msg;
      public int wParam;
      public int lParam;
      public int time;
      public int pt;
    }
  }
}
