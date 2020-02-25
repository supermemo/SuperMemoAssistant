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
// Modified On:  2020/02/03 00:24
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Permissions;
using System.Threading.Tasks;
using Anotar.Serilog;
using SuperMemoAssistant.Exceptions;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.SMA.Configs;
using SuperMemoAssistant.SuperMemo;
using Extensions.System.IO;
using SuperMemoAssistant.Sys.Windows.Search;

namespace SuperMemoAssistant.SMA.Utils
{
  public static class SuperMemoFinder
  {
    #region Constants & Statics

    private static readonly DirectoryPath[] SuperMemoFolderNames  = { "SuperMemo", "SuperMemo17", "SuperMemo18" };
    private static readonly string[]        SuperMemoExeFileNames = { "sm17.exe", "sm18.exe" };

    #endregion




    #region Methods

    public static List<FilePath> SearchSuperMemoInDefaultLocations()
    {
      try
      {
        var smExePaths = new List<FilePath>();

        foreach (var rootDirPath in ListRootDirPaths())
        foreach (var smFolderName in SuperMemoFolderNames)
        foreach (var smExeFileName in SuperMemoExeFileNames)
          try
          {
            var smExePath = rootDirPath.Combine(smFolderName).CombineFile(smExeFileName);

            if (smExePath.Exists())
              smExePaths.Add(smExePath);
          }
          catch (Exception ex)
          {
            LogTo.Error(ex, "Exception thrown while checking existence of a SuperMemo executable path");
          }

        return smExePaths;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception thrown while searching for SuperMemo executables");

        return null;
      }
    }

    public static async Task<List<FilePath>> SearchSuperMemoInWindowsIndex()
    {
      if (WindowsSearch.Instance.IsAvailable == false)
      {
        LogTo.Warning("Windows search is unavailable. Searching for SuperMemo executable aborted");
        return new List<FilePath>();
      }

      var wsRes = await WindowsSearch.Instance.Search("sm1%.exe", WindowsSearchKind.Program);

      return wsRes.Select(wsr => new FilePath(wsr.FilePath))
                  .ToList();
    }

    public static bool CheckSuperMemoExecutable(
      NativeDataCfg    nativeDataCfg,
      FilePath         smFile,
      out NativeData   nativeData,
      out SMAException ex)
    {
      nativeData = null;

      if (smFile == null)
      {
        ex = new SMAException("SM exe file path is null", new ArgumentNullException(nameof(smFile)));
        return false;
      }

      if (smFile.Exists() == false)
      {
        ex = new SMAException(
          $"Invalid file path for sm executable file: '{smFile}' could not be found. SMA cannot continue.");
        return false;
      }

      if (smFile.HasPermission(FileIOPermissionAccess.Read) == false)
      {
        ex = new SMAException($"SMA needs read access to execute SM executable at {smFile.FullPath}.");
        return false;
      }

      if (smFile.IsLocked())
      {
        ex = new SMAException($"{smFile.FullPath} is locked. Make sure it isn't already running.");
        return false;
      }

      var smFileCrc32 = FileEx.GetCrc32(smFile.FullPath);
      nativeData = nativeDataCfg.SafeGet(smFileCrc32.ToUpper(CultureInfo.InvariantCulture));

      if (nativeData == null)
      {
        ex = new SMAException($"Unknown SM executable version with crc32 {smFileCrc32}.");
        return false;
      }

      ex = null;

      return true;
    }

    private static IEnumerable<DirectoryPath> ListRootDirPaths()
    {
      var userRootDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

      var folderPaths = new[]
      {
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        userRootDir,
        Path.Combine(userRootDir, "Google Drive"),
        Path.Combine(userRootDir, "Dropbox"),
        Path.Combine(userRootDir, "OneDrive"),
      };

      return folderPaths.Concat(ListDrives())
                        .Select(fp => new DirectoryPath(fp));
    }


    public static List<string> ListDrives()
    {
      var drives = new List<string>();

      using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Volume"))
      {
        ManagementObjectCollection collection = searcher.Get();

        foreach (var mbo in collection)
          if (mbo is ManagementObject mo)
            drives.Add((string)mo["DriveLetter"]);
      }

      return drives.Where(d => string.IsNullOrWhiteSpace(d) == false)
                   .ToList();
    }

    #endregion
  }
}
