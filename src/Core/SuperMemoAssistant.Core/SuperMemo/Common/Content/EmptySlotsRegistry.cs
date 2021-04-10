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




namespace SuperMemoAssistant.SuperMemo.Common.Content
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using Anotar.Serilog;
  using Extensions;
  using global::Extensions.System.IO;
  using Hooks;
  using Interop;
  using Microsoft.Toolkit.Uwp.Notifications;
  using Registry;
  using SMA;
  using SuperMemoAssistant.Extensions;
  using SuperMemoAssistant.Interop.SuperMemo.Core;
  using Sys.SparseClusteredArray;
  using Sys.Windows;

  public class EmptySlotsRegistry : SMHookIOBase
  {
    #region Constants & Statics

    private static readonly IEnumerable<string> TargetFiles = new[]
    {
      SMConst.Files.EmptySlotsFile
    };

    #endregion




    #region Properties & Fields - Non-Public

    private static string RecoveryFolder => Collection.CombinePath(SMConst.Paths.RecoverFolder);

    private static string EmptySlotsFilePath => Collection.GetInfoFilePath(SMConst.Files.EmptySlotsFile);

    private SparseClusteredArray<byte> EmptySlotsSCA { get; } = new SparseClusteredArray<byte>();

    #endregion




    #region Methods Impl

    protected override void Cleanup()
    {
      EmptySlotsSCA.Clear();
    }

    protected override void CommitFromFiles()
    {
      using (Stream stream = File.OpenRead(EmptySlotsFilePath))
        ParseAndCheck(stream);
    }

    protected override void CommitFromMemory()
    {
      foreach (SegmentStream stream in EmptySlotsSCA.GetStreams())
        ParseAndCheck(stream);
    }

    protected override SparseClusteredArray<byte> GetSCAForFileName(string fileName)
    {
      switch (fileName)
      {
        case SMConst.Files.EmptySlotsFile:
          return EmptySlotsSCA;

        default:
          return null;
      }
    }

    public override IEnumerable<string> GetTargetFilePaths()
    {
      return TargetFiles.Select(f => Collection.GetInfoFilePath(f));
    }

    #endregion




    #region Methods

    private static void ParseAndCheck(Stream stream)
    {
      var recoverDirPath = new DirectoryPath(RecoveryFolder);

      recoverDirPath.EnsureExists();

      int r, moved = 0;
      var buffer = new byte[4096];

      while ((r = stream.Read(buffer, 0, buffer.Length)) > 0)
        for (r--; r >= 0; r--)
        {
          var slotId = Convert.ToInt32(buffer[r]);

          if (CheckAndMoveNonEmptyFileSlots(slotId, recoverDirPath))
            moved++;
        }

      if (moved > 0)
        SendFileMovedNotification(moved);
    }

    private static bool CheckAndMoveNonEmptyFileSlots(int slotId, DirectoryPath recoverDirPath)
    {
      var wildCardPath     = RegistryMemberBase.GetFilePathForSlotId(Core.SM.Collection, slotId, "*");
      var fileNameWildCard = Path.GetFileName(wildCardPath);
      var dirPath          = Path.GetDirectoryName(wildCardPath);

      fileNameWildCard.ThrowIfNullOrWhitespace("fileNameWildCard was null or whitespace");
      dirPath.ThrowIfNullOrWhitespace("dirPath was null or whitespace");

      recoverDirPath.EnsureExists();

      bool moved = false;

      // TODO: Does there need to be a loop - only looking for one file?
      foreach (var filePath in Directory.EnumerateFiles(dirPath, fileNameWildCard, SearchOption.TopDirectoryOnly))
        try
        {
          // TODO: What if there is already a file in recover dir with the same name

          var date     = DateTime.Today;
          var fileName = $"{date.Day}-{date.Month}-{date.Year}_{Path.GetFileName(filePath)}";
          File.Move(filePath, recoverDirPath.CombineFile(fileName).FullPath);
          moved = true;
        }
        catch (IOException ex)
        {
          LogTo.Warning(ex, "Failed to move non-empty fileslot file {FilePath}", filePath);

          if (File.Exists(filePath))
            throw new InvalidOperationException($"Failed to remove non-empty fileslot file {filePath}");
        }

      return moved;
    }

    private static void SendFileMovedNotification(int number)
    {
      $"{number} files were found occupying an empty file slot. They were moved to the recovery folder."
        .ShowDesktopNotification(
          new ToastButton("Open the recovery folder.", RecoveryFolder)
          {
            ActivationType = ToastActivationType.Protocol
          }
        );
    }

    #endregion
  }
}
