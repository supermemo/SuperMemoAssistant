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
// Created On:   2018/05/12 01:42
// Modified On:  2018/12/13 12:58
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EasyHook;

namespace SuperMemoAssistant.Hooks.InjectLib
{
  public static class IOHooks
  {
    #region Methods

    //
    // Setup

    internal static IEnumerable<LocalHook> InstallHooks()
    {
      return new[]
      {
        LocalHook.Create(
          LocalHook.GetProcAddress("kernel32.dll",
                                   "CreateFileW"),
          new CreateFileWDlg(CreateFile_Hooked),
          SMInject.Instance
        ),
        LocalHook.Create(
          LocalHook.GetProcAddress("kernel32.dll",
                                   "SetFilePointer"),
          new SetFilePointerDlg(SetFilePointer_Hooked),
          SMInject.Instance
        ),
        LocalHook.Create(
          LocalHook.GetProcAddress("kernel32.dll",
                                   "WriteFile"),
          new WriteFileDlg(WriteFile_Hooked),
          SMInject.Instance
        ),
        LocalHook.Create(
          LocalHook.GetProcAddress("kernel32.dll",
                                   "CloseHandle"),
          new CloseHandleDlg(CloseHandle_Hooked),
          SMInject.Instance
        )
      };
    }


    //
    // Hooked methods

    private static IntPtr CreateFile_Hooked(
      String inFileName,
      UInt32 inDesiredAccess,
      UInt32 inShareMode,
      IntPtr inSecurityAttributes,
      UInt32 inCreationDisposition,
      UInt32 inFlagsAndAttributes,
      IntPtr inTemplateFile)
    {
      IntPtr result = CreateFileW(
        inFileName,
        inDesiredAccess,
        inShareMode,
        inSecurityAttributes,
        inCreationDisposition,
        inFlagsAndAttributes,
        inTemplateFile
      );

      try
      {
        if (SMInject.Instance.TargetFilePaths.Contains(inFileName.ToLowerInvariant()))
        {
          SMInject.Instance.TargetHandles.Add(result);
          SMInject.Instance.Enqueue("CreateFile",
                                    inFileName,
                                    result);
        }
      }
      catch (Exception ex)
      {
        SMInject.Instance.OnException(ex);
      }

      return result;
    }

    private static UInt32 SetFilePointer_Hooked(
      IntPtr     inFileHandle,
      Int32      inDistanceToMove,
      ref IntPtr inOutDistanceToMoveHigh,
      UInt32     inMoveMethod)
    {
      UInt32 position = SetFilePointer(
        inFileHandle,
        inDistanceToMove,
        ref inOutDistanceToMoveHigh,
        inMoveMethod
      );

      try
      {
        if (SMInject.Instance.TargetHandles.Contains(inFileHandle))
          SMInject.Instance.Enqueue("SetFilePointer",
                                    inFileHandle,
                                    position);
      }
      catch (Exception ex)
      {
        SMInject.Instance.OnException(ex);
      }

      return position;
    }

    private static Boolean WriteFile_Hooked(
      IntPtr     inFileHandle,
      IntPtr     inBuffer,
      UInt32     inNumberOfBytesToWrite,
      out IntPtr outNumberOfBytesWritten,
      IntPtr     inOutOverlapped)
    {
      try
      {
        if (SMInject.Instance.TargetHandles.Contains(inFileHandle))
        {
          byte[] buffer = new byte[inNumberOfBytesToWrite];
          Marshal.Copy(inBuffer,
                       buffer,
                       0,
                       (int)inNumberOfBytesToWrite);

          SMInject.Instance.Enqueue("WriteFile",
                                    inFileHandle,
                                    buffer,
                                    inNumberOfBytesToWrite);
        }
      }
      catch (Exception ex)
      {
        SMInject.Instance.OnException(ex);
      }

      return WriteFile(
        inFileHandle,
        inBuffer,
        inNumberOfBytesToWrite,
        out outNumberOfBytesWritten,
        inOutOverlapped
      );
    }

    private static Boolean CloseHandle_Hooked(
      IntPtr inFileHandle)
    {
      try
      {
        if (SMInject.Instance.TargetHandles.Contains(inFileHandle))
        {
          SMInject.Instance.TargetHandles.Remove(inFileHandle);
          SMInject.Instance.Enqueue("CloseHandle",
                                    inFileHandle);
        }
      }
      catch (Exception ex)
      {
        SMInject.Instance.OnException(ex);
      }

      return CloseHandle(inFileHandle);
    }


    //
    // Native APIs

    [DllImport("kernel32.dll",
      CallingConvention = CallingConvention.StdCall,
      CharSet           = CharSet.Unicode,
      SetLastError      = true)]
    private static extern IntPtr CreateFileW(
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
    private static extern UInt32 SetFilePointer(
      IntPtr     inFileHandle,
      Int32      inDistanceToMove,
      ref IntPtr inOutDistanceToMoveHigh,
      UInt32     inMoveMethod);

    [DllImport("kernel32.dll",
      CallingConvention = CallingConvention.StdCall,
      CharSet           = CharSet.Unicode,
      SetLastError      = true)]
    private static extern Boolean WriteFile(
      IntPtr     inFileHandle,
      IntPtr     inBuffer,
      UInt32     inNumberOfBytesToWrite,
      out IntPtr outNumberOfBytesWritten,
      IntPtr     inOutOverlapped);

    [DllImport("kernel32.dll",
      CallingConvention = CallingConvention.StdCall,
      CharSet           = CharSet.Unicode,
      SetLastError      = true)]
    private static extern Boolean CloseHandle(
      IntPtr inFileHandle);

    #endregion




    //
    // Delegates

    [UnmanagedFunctionPointer(CallingConvention.StdCall,
      CharSet      = CharSet.Unicode,
      SetLastError = true)]
    private delegate IntPtr CreateFileWDlg(
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
    private delegate UInt32 SetFilePointerDlg(
      IntPtr     inFileHandle,
      Int32      inDistanceToMove,
      ref IntPtr inOutDistanceToMoveHigh,
      UInt32     inMoveMethod);

    [UnmanagedFunctionPointer(CallingConvention.StdCall,
      CharSet      = CharSet.Unicode,
      SetLastError = true)]
    private delegate Boolean WriteFileDlg(
      IntPtr     inFileHandle,
      IntPtr     inBuffer,
      UInt32     inNumberOfBytesToWrite,
      out IntPtr outNumberOfBytesWritten,
      IntPtr     inOutOverlapped);

    [UnmanagedFunctionPointer(CallingConvention.StdCall,
      CharSet      = CharSet.Unicode,
      SetLastError = true)]
    private delegate Boolean CloseHandleDlg(
      IntPtr inFileHandle);
  }
}
