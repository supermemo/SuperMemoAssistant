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

#endregion




namespace SuperMemoAssistant.Plugins.DevSandbox
{
  using System;
  using System.IO;
  using System.Runtime.InteropServices;
  using Microsoft.Win32.SafeHandles;
  
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
  internal static class Kernel32
  {
    #region Constants & Statics

    private const UInt32 GENERIC_WRITE         = 0x40000000;
    private const UInt32 GENERIC_READ          = 0x80000000;
    private const UInt32 FILE_SHARE_READ       = 0x00000001;
    private const UInt32 FILE_SHARE_WRITE      = 0x00000002;
    private const UInt32 OPEN_EXISTING         = 0x00000003;
    private const UInt32 FILE_ATTRIBUTE_NORMAL = 0x80;
    private const UInt32 ERROR_ACCESS_DENIED   = 5;

    private const UInt32 ATTACH_PARRENT = 0xFFFFFFFF;

    #endregion




    #region Methods

    public static void CreateConsole(bool alwaysCreateNewConsole = true)
    {
      bool consoleAttached = true;
      if (alwaysCreateNewConsole
        || AttachConsole(ATTACH_PARRENT) == 0
        && Marshal.GetLastWin32Error()   != ERROR_ACCESS_DENIED)
        consoleAttached = AllocConsole() != 0;

      if (consoleAttached)
      {
        InitializeOutStream();
        InitializeInStream();
      }
    }

    private static void InitializeOutStream()
    {
      var fs = CreateFileStream("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, FileAccess.Write);
      if (fs != null)
      {
        var writer = new StreamWriter(fs) { AutoFlush = true };
        Console.SetOut(writer);
        Console.SetError(writer);
      }
    }

    private static void InitializeInStream()
    {
      var fs = CreateFileStream("CONIN$", GENERIC_READ, FILE_SHARE_READ, FileAccess.Read);
      if (fs != null)
        Console.SetIn(new StreamReader(fs));
    }

    private static FileStream CreateFileStream(string     name,
                                               uint       win32DesiredAccess,
                                               uint       win32ShareMode,
                                               FileAccess dotNetFileAccess)
    {
      var file = new SafeFileHandle(
        CreateFileW(name, win32DesiredAccess, win32ShareMode, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero),
        true
      );

      if (!file.IsInvalid)
      {
        var fs = new FileStream(file, dotNetFileAccess);
        return fs;
      }

      return null;
    }

    [DllImport("kernel32.dll",
               EntryPoint        = "AllocConsole",
               SetLastError      = true,
               CharSet           = CharSet.Auto,
               CallingConvention = CallingConvention.StdCall)]
    private static extern int AllocConsole();

    [DllImport("kernel32.dll")]
    public static extern bool FreeConsole();

    [DllImport("kernel32.dll",
               EntryPoint        = "AttachConsole",
               SetLastError      = true,
               CharSet           = CharSet.Auto,
               CallingConvention = CallingConvention.StdCall)]
    private static extern UInt32 AttachConsole(UInt32 dwProcessId);

    [DllImport("kernel32.dll",
               EntryPoint        = "CreateFileW",
               SetLastError      = true,
               CharSet           = CharSet.Unicode,
               CallingConvention = CallingConvention.StdCall)]
    private static extern IntPtr CreateFileW(
      string lpFileName,
      UInt32 dwDesiredAccess,
      UInt32 dwShareMode,
      IntPtr lpSecurityAttributes,
      UInt32 dwCreationDisposition,
      UInt32 dwFlagsAndAttributes,
      IntPtr hTemplateFile
    );

    #endregion
  }
}
