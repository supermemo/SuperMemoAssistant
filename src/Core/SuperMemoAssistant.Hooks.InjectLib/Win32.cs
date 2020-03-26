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
// Modified On:  2020/03/13 13:26
// Modified By:  Alexis

#endregion




using System;
using System.Runtime.InteropServices;

namespace SuperMemoAssistant.Hooks.InjectLib
{
  internal static class Win32
  {
    #region Methods

    [DllImport("user32.dll",
               CharSet = CharSet.Auto)]
    public static extern bool ShowWindow(IntPtr inHwnd, int inNCmdShow);

    [DllImport("kernel32.dll",
               CallingConvention = CallingConvention.StdCall,
               CharSet           = CharSet.Unicode,
               SetLastError      = true)]
    public static extern IntPtr CreateFileW(
      String inFileName,
      UInt32 inDesiredAccess,
      UInt32 inShareMode,
      IntPtr inSecurityAttributes,
      UInt32 inCreationDisposition,
      UInt32 inFlagsAndAttributes,
      IntPtr inTemplateFile);

    [DllImport("kernel32.dll",
               CallingConvention = CallingConvention.StdCall,
               CharSet           = CharSet.Unicode,
               SetLastError      = true)]
    public static extern UInt32 SetFilePointer(
      IntPtr     inFileHandle,
      Int32      inDistanceToMove,
      ref IntPtr inOutDistanceToMoveHigh,
      UInt32     inMoveMethod);

    [DllImport("kernel32.dll",
               CallingConvention = CallingConvention.StdCall,
               CharSet           = CharSet.Unicode,
               SetLastError      = true)]
    public static extern Boolean WriteFile(
      IntPtr     inFileHandle,
      IntPtr     inBuffer,
      UInt32     inNumberOfBytesToWrite,
      out IntPtr outNumberOfBytesWritten,
      IntPtr     inOutOverlapped);

    [DllImport("kernel32.dll",
               CallingConvention = CallingConvention.StdCall,
               CharSet           = CharSet.Unicode,
               SetLastError      = true)]
    public static extern Boolean CloseHandle(
      IntPtr inFileHandle);

    #endregion




    //
    // Delegates
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall,
                              CharSet      = CharSet.Unicode,
                              SetLastError = true)]
    public delegate bool CreateShowWindowDlg(IntPtr inHwnd, int inNCmdShow);

    [UnmanagedFunctionPointer(CallingConvention.StdCall,
                              CharSet      = CharSet.Unicode,
                              SetLastError = true)]
    public delegate IntPtr CreateFileWDlg(
      String inFileName,
      UInt32 inDesiredAccess,
      UInt32 inShareMode,
      IntPtr inSecurityAttributes,
      UInt32 inCreationDisposition,
      UInt32 inFlagsAndAttributes,
      IntPtr inTemplateFile);

    [UnmanagedFunctionPointer(CallingConvention.StdCall,
                              CharSet      = CharSet.Unicode,
                              SetLastError = true)]
    public delegate UInt32 SetFilePointerDlg(
      IntPtr     inFileHandle,
      Int32      inDistanceToMove,
      ref IntPtr inOutDistanceToMoveHigh,
      UInt32     inMoveMethod);

    [UnmanagedFunctionPointer(CallingConvention.StdCall,
                              CharSet      = CharSet.Unicode,
                              SetLastError = true)]
    public delegate Boolean WriteFileDlg(
      IntPtr     inFileHandle,
      IntPtr     inBuffer,
      UInt32     inNumberOfBytesToWrite,
      out IntPtr outNumberOfBytesWritten,
      IntPtr     inOutOverlapped);

    [UnmanagedFunctionPointer(CallingConvention.StdCall,
                              CharSet      = CharSet.Unicode,
                              SetLastError = true)]
    public delegate Boolean CloseHandleDlg(
      IntPtr inFileHandle);
  }
}
