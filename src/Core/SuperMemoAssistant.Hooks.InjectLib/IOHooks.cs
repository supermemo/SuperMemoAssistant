using EasyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Hooks.InjectLib
{
  public static class IOHooks
  {
    //
    // Setup

    internal static IEnumerable<LocalHook> InstallHooks()
    {
      return new[] {
        LocalHook.Create(
          LocalHook.GetProcAddress("kernel32.dll", "CreateFileW"),
          new CreateFileWDlg(CreateFile_Hooked),
          SMInject.Instance
        ),
        LocalHook.Create(
          LocalHook.GetProcAddress("kernel32.dll", "SetFilePointer"),
          new SetFilePointerDlg(SetFilePointer_Hooked),
          SMInject.Instance
        ),
        LocalHook.Create(
          LocalHook.GetProcAddress("kernel32.dll", "WriteFile"),
          new WriteFileDlg(WriteFile_Hooked),
          SMInject.Instance
        ),
        LocalHook.Create(
          LocalHook.GetProcAddress("kernel32.dll", "CloseHandle"),
          new CloseHandleDlg(CloseHandle_Hooked),
          SMInject.Instance
        )
      };
    }



    //
    // Hooked methods

    static IntPtr CreateFile_Hooked(
      String InFileName,
      UInt32 InDesiredAccess,
      UInt32 InShareMode,
      IntPtr InSecurityAttributes,
      UInt32 InCreationDisposition,
      UInt32 InFlagsAndAttributes,
      IntPtr InTemplateFile)
    {
      IntPtr result = CreateFileW(
        InFileName,
        InDesiredAccess,
        InShareMode,
        InSecurityAttributes,
        InCreationDisposition,
        InFlagsAndAttributes,
        InTemplateFile
      );

      try
      {
        if (SMInject.Instance.TargetFilePaths.Contains(InFileName.ToLowerInvariant()))
        {
          SMInject.Instance.TargetHandles.Add(result);
          SMInject.Instance.Enqueue("CreateFile", InFileName, result);
        }
      }
      catch
      {
      }

      return result;
    }

    static UInt32 SetFilePointer_Hooked(
          IntPtr InFileHandle,
          Int32  InDistanceToMove,
          ref IntPtr InOutDistanceToMoveHigh,
          UInt32 InMoveMethod)
    {
      UInt32 position = SetFilePointer(
        InFileHandle,
        InDistanceToMove,
        ref InOutDistanceToMoveHigh,
        InMoveMethod
      );

      try
      {
        if (SMInject.Instance.TargetHandles.Contains(InFileHandle))
          SMInject.Instance.Enqueue("SetFilePointer", InFileHandle, position);
      }
      catch
      {
      }

      return position;
    }

    static Boolean WriteFile_Hooked(
          IntPtr InFileHandle,
          IntPtr InBuffer,
          UInt32 InNumberOfBytesToWrite,
          out IntPtr OutNumberOfBytesWritten,
          IntPtr InOutOverlapped)
    {
      try
      {
        if (SMInject.Instance.TargetHandles.Contains(InFileHandle))
        {
          byte[] buffer = new byte[InNumberOfBytesToWrite];
          Marshal.Copy(InBuffer, buffer, 0, (int)InNumberOfBytesToWrite);

          SMInject.Instance.Enqueue("WriteFile", InFileHandle, buffer, InNumberOfBytesToWrite);
        }
      }
      catch
      {
      }

      return WriteFile(
        InFileHandle,
        InBuffer,
        InNumberOfBytesToWrite,
        out OutNumberOfBytesWritten,
        InOutOverlapped
      );
    }

    static Boolean CloseHandle_Hooked(
          IntPtr InFileHandle)
    {
      try
      {
        if (SMInject.Instance.TargetHandles.Contains(InFileHandle))
        {
          SMInject.Instance.TargetHandles.Remove(InFileHandle);
          SMInject.Instance.Enqueue("CloseHandle", InFileHandle);
        }
      }
      catch
      {
      }

      return CloseHandle(InFileHandle);
    }



    //
    // Delegates

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate IntPtr CreateFileWDlg(
        String InFileName,
        UInt32 InDesiredAccess,
        UInt32 InShareMode,
        IntPtr InSecurityAttributes,
        UInt32 InCreationDisposition,
        UInt32 InFlagsAndAttributes,
        IntPtr InTemplateFile);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate UInt32 SetFilePointerDlg(
          IntPtr InFileHandle,
          Int32  InDistanceToMove,
          ref IntPtr InOutDistanceToMoveHigh,
          UInt32 InMoveMethod);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate Boolean WriteFileDlg(
          IntPtr InFileHandle,
          IntPtr InBuffer,
          UInt32 InNumberOfBytesToWrite,
          out IntPtr OutNumberOfBytesWritten,
          IntPtr InOutOverlapped);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate Boolean CloseHandleDlg(
          IntPtr InFileHandle);



    //
    // Native APIs

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    static extern IntPtr CreateFileW(
        String InFileName,
        UInt32 InDesiredAccess,
        UInt32 InShareMode,
        IntPtr InSecurityAttributes,
        UInt32 InCreationDisposition,
        UInt32 InFlagsAndAttributes,
        IntPtr InTemplateFile);

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    static extern UInt32 SetFilePointer(
          IntPtr InFileHandle,
          Int32  InDistanceToMove,
          ref IntPtr InOutDistanceToMoveHigh,
          UInt32 InMoveMethod);

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    static extern Boolean WriteFile(
          IntPtr InFileHandle,
          IntPtr InBuffer,
          UInt32 InNumberOfBytesToWrite,
          out IntPtr OutNumberOfBytesWritten,
          IntPtr InOutOverlapped);

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    static extern Boolean CloseHandle(
          IntPtr InFileHandle);
  }
}
