using Anotar.Serilog;
using Extensions.System.IO;
using Microsoft.Toolkit.Uwp.Notifications;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SuperMemo.Common.Extensions;
using SuperMemoAssistant.SuperMemo.Common.Registry;
using SuperMemoAssistant.SuperMemo.Hooks;
using SuperMemoAssistant.Sys.SparseClusteredArray;
using SuperMemoAssistant.Sys.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SuperMemoAssistant.SuperMemo.Common.Content
{
  public class EmptySlotsRegistry : SMHookIOBase
  {

    // TODO: Should these be placed in SMConst?
    private const string EmptySlotsFile = "emptyslots.dat";
    private readonly string RecoveryFolder = Path.Combine(Core.SM.Collection.Path, "recover");

    protected static readonly IEnumerable<string> TargetFiles = new string[]
    {
      EmptySlotsFile
    };

    protected SparseClusteredArray<byte> EmptySlotsSCA { get; } = new SparseClusteredArray<byte>();

    protected override void Cleanup()
    {
      EmptySlotsSCA.Clear();
    }

    private void ParseAndCheck(Stream stream)
    {
      using (BinaryReader binaryReader = new BinaryReader(stream))
      {

        // TODO: Only seems to read one fileslot

        int moved = 0;
        while (binaryReader.PeekChar() != -1)
        {
          var slotId = binaryReader.ReadInt32();
          if (CheckAndMoveNonEmptyFileSlots(slotId))
            moved++;
        }

        if (moved > 0)
          SendFileMovedNotification(moved);
      }
    }

    private void SendFileMovedNotification(int number)
    {
      string message = $"{number} files were found occupying an empty file slot. " +
                        "They were moved to the recovery folder.";

      message.ShowDesktopNotification(
        new ToastButton("Open the recovery folder.", RecoveryFolder)
        {
          ActivationType = ToastActivationType.Protocol
        }
      );

    }

    protected override void CommitFromFiles()
    {
      using (Stream stream = File.OpenRead(Collection.GetInfoFilePath(EmptySlotsFile)))
        ParseAndCheck(stream);
    }

    private bool CheckAndMoveNonEmptyFileSlots(int slotId)
    {

      //var memory = Core.SM.SMProcess.Memory; // NRE here
      //var fileSpaceInst = Core.Natives.FileSpace.InstancePtr.Read<IntPtr>(memory);
      //var emptySlots = Core.Natives.FileSpace.EmptySlotsPtr.Read<IntPtr>(memory);
      //if (Core.Natives.FileSpace.IsSlotOccupied.Invoke(fileSpaceInst, slotId))
      //{

      var recoverDirPath = new DirectoryPath(RecoveryFolder);
      var wildCardPath = RegistryMemberBase.GetFilePathForSlotId(Core.SM.Collection, slotId, "*");
      var fileNameWildCard = Path.GetFileName(wildCardPath);
      var dirPath = Path.GetDirectoryName(wildCardPath);

      fileNameWildCard.ThrowIfNullOrWhitespace("fileNameWildCard was null or whitespace");
      dirPath.ThrowIfNullOrWhitespace("dirPath was null or whitespace");

      recoverDirPath.EnsureExists();

      bool moved = false;

      // TODO: Does there need to be a loop - only looking for one file?
      foreach (var filePath in Directory.EnumerateFiles(dirPath, fileNameWildCard, SearchOption.TopDirectoryOnly))
      {
        try
        {

          // TODO: What if there is already a file in recover dir with the same name

          var date = DateTime.Today;
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
      }

      return moved;
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
        case EmptySlotsFile:
          return EmptySlotsSCA;

        default:
          return null;
      }
    }

    public override IEnumerable<string> GetTargetFilePaths()
    {
      return TargetFiles.Select(f => Collection.GetInfoFilePath(f));
    }
  }
}
