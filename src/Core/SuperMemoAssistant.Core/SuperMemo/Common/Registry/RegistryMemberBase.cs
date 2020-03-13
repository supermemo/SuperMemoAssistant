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
// Created On:   2019/08/07 14:43
// Modified On:  2019/08/07 14:47
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Anotar.Serilog;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SuperMemo.Common.Registry.Models;

namespace SuperMemoAssistant.SuperMemo.Common.Registry
{
  public abstract class RegistryMemberBase : MarshalByRefObject, INotifyPropertyChanged
  {
    #region Constructors

    protected RegistryMemberBase(int id)
    {
#if DEBUG && !DEBUG_IN_PROD
      LogTo.Debug("[{0} {1}] Creating",
                  GetType().Name,
                  id);
#endif

      Id = id;
    }

    #endregion




    #region Properties & Fields - Public

    public int Id { get; }

    public int UseCount { get; set; }

    public RegistryLinkType LinkType { get; set; }

    public int    RtxId     { get; set; }
    public int    RtxOffset { get; set; }
    public int    RtxLength { get; set; }
    public string RtxValue  { get; set; }

    public int SlotIdOrOffset             { get; set; }
    public int SlotLengthOrConceptGroupId { get; set; }

    public bool Empty { get; set; }

    public string Name => RtxValue?.TrimEnd('\0');

    #endregion




    #region Methods

    /// <summary>
    ///   Raises the <see cref="PropertyChanged" /> event for Property
    ///   <paramref name="propertyName" />. Called by Fody.PropertyChanged
    /// </summary>
    /// <param name="propertyName">The changed property's name</param>
    /// <param name="before">The old value</param>
    /// <param name="after">The new value</param>
    protected void OnPropertyChanged(string propertyName, object before, object after)
    {
#if DEBUG && !DEBUG_IN_PROD
      LogTo.Debug("[{0} {1}] {2}: {3}",
                  GetType().Name,
                  Id,
                  propertyName,
                  after);
#endif

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string TryFilePathOrSearch(string fileExt)
    {
      var filePath = GetFilePath(fileExt);

      if (File.Exists(filePath))
        return filePath;

      return GetFilePath();
    }

    public virtual string GetFilePath()
    {
      try
      {
        var filePath = GetFilePath(".");
        var fileName = Path.GetFileName(filePath);
        var dirPath  = Path.GetDirectoryName(filePath);

        // ReSharper disable once AssignNullToNotNullAttribute
        var matchingFiles = Directory.GetFiles(dirPath,
                                               fileName + "*");
        return matchingFiles.FirstOrDefault();
      }
      catch (Exception ex)
      {
        LogTo.Warning(ex,
                      $"Failed to get file path for {GetType().Name} {Id} \"{Name}\"");
        return null;
      }
    }

    public string GetFilePath(string fileExt)
    {
      SMCollection collection = Core.SM.Collection;

      return GetFilePathForSlotId(
        collection,
        SlotIdOrOffset,
        fileExt
      );

      /*
      switch (LinkType)
      {
        case RegistryLinkType.File:
        case RegistryLinkType.FileAndRtx:
          SMCollection collection = SMA.Instance.Collection;

          return GetFilePathForSlotId(
            collection,
            SlotIdOrOffset,
            fileExt
          );

        default:
          return null;
      }*/
    }

    protected static string GetFilePathForSlotId(
      SMCollection collection,
      int          slotId,
      string       slotFileExt)
    {
      if (slotId <= 10)
        return collection.GetElementFilePath($"{slotId}.{slotFileExt}");

      List<int> folders = new List<int>();
      int       nBranch = (int)Math.Floor((slotId - 1) / 10.0);

      do
      {
        folders.Add((nBranch - 1) % 30 + 1);

        nBranch = (int)Math.Floor((nBranch - 1) / 30.0);
      } while (nBranch > 0);

      folders.Reverse();
      var folderPath = string.Join("\\", folders);

      return collection.GetElementFilePath(
        Path.Combine(
          folderPath,
          $"{slotId}.{slotFileExt}"
        )
      );
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
