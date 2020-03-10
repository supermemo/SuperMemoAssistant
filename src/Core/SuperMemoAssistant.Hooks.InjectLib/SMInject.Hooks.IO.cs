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
// Modified On:  2019/02/24 23:42
// Modified By:  Alexis

#endregion




using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using EasyHook;

namespace SuperMemoAssistant.Hooks.InjectLib
{
  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class SMInject
  {
    #region Properties & Fields - Public

    public HashSet<string> TargetFilePaths { get; set; } = new HashSet<string>();
    public HashSet<IntPtr> TargetHandles   { get; }      = new HashSet<IntPtr>();

    #endregion




    #region Methods

    //
    // Setup

    private IEnumerable<LocalHook> InstallIOHooks()
    {
      TargetFilePaths = new HashSet<string>(SMA.GetTargetFilePaths().Select(s => s.ToLowerInvariant()));

      return new[]
      {
        LocalHook.Create(
          LocalHook.GetProcAddress("kernel32.dll", "CreateFileW"),
          new Win32.CreateFileWDlg(CreateFile_Hooked),
          this
        ),
        LocalHook.Create(
          LocalHook.GetProcAddress("kernel32.dll", "SetFilePointer"),
          new Win32.SetFilePointerDlg(SetFilePointer_Hooked),
          this
        ),
        LocalHook.Create(
          LocalHook.GetProcAddress("kernel32.dll", "WriteFile"),
          new Win32.WriteFileDlg(WriteFile_Hooked),
          this
        ),
        LocalHook.Create(
          LocalHook.GetProcAddress("kernel32.dll", "CloseHandle"),
          new Win32.CloseHandleDlg(CloseHandle_Hooked),
          this
        )
      };
    }


    //
    // Hooked methods

    private IntPtr CreateFile_Hooked(
      String inFileName,
      UInt32 inDesiredAccess,
      UInt32 inShareMode,
      IntPtr inSecurityAttributes,
      UInt32 inCreationDisposition,
      UInt32 inFlagsAndAttributes,
      IntPtr inTemplateFile)
    {
      IntPtr result = Win32.CreateFileW(
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
        if (TargetFilePaths.Contains(inFileName.ToLowerInvariant()))
        {
          TargetHandles.Add(result);
          Enqueue(HookedFunction.CreateFile,
                  inFileName,
                  result);
        }
      }
      catch (Exception ex)
      {
        OnException(ex);
      }

      return result;
    }

    private UInt32 SetFilePointer_Hooked(
      IntPtr     inFileHandle,
      Int32      inDistanceToMove,
      ref IntPtr inOutDistanceToMoveHigh,
      UInt32     inMoveMethod)
    {
      UInt32 position = Win32.SetFilePointer(
        inFileHandle,
        inDistanceToMove,
        ref inOutDistanceToMoveHigh,
        inMoveMethod
      );

      try
      {
        if (TargetHandles.Contains(inFileHandle))
          Enqueue(HookedFunction.SetFilePointer,
                  inFileHandle,
                  position);
      }
      catch (Exception ex)
      {
        OnException(ex);
      }

      return position;
    }

    private Boolean WriteFile_Hooked(
      IntPtr     inFileHandle,
      IntPtr     inBuffer,
      UInt32     inNumberOfBytesToWrite,
      out IntPtr outNumberOfBytesWritten,
      IntPtr     inOutOverlapped)
    {
      byte[] buffer = null;

      try
      {
        if (TargetHandles.Contains(inFileHandle))
        {
          buffer = ArrayPool<byte>.Shared.Rent((int)inNumberOfBytesToWrite);

          Marshal.Copy(inBuffer,
                       buffer,
                       0,
                       (int)inNumberOfBytesToWrite);

          Enqueue(HookedFunction.WriteFile,
                  inFileHandle,
                  buffer,
                  inNumberOfBytesToWrite);
        }
      }
      catch (Exception ex)
      {
        OnException(ex);

        if (buffer != null)
          ArrayPool<byte>.Shared.Return(buffer);
      }

      return Win32.WriteFile(
        inFileHandle,
        inBuffer,
        inNumberOfBytesToWrite,
        out outNumberOfBytesWritten,
        inOutOverlapped
      );
    }

    private Boolean CloseHandle_Hooked(
      IntPtr inFileHandle)
    {
      try
      {
        if (TargetHandles.Contains(inFileHandle))
        {
          TargetHandles.Remove(inFileHandle);
          Enqueue(HookedFunction.CloseHandle,
                  inFileHandle);
        }
      }
      catch (Exception ex)
      {
        OnException(ex);
      }

      return Win32.CloseHandle(inFileHandle);
    }

    #endregion
  }
}
