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
// Created On:   2019/05/08 19:50
// Modified On:  2020/01/12 10:14
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SuperMemo.Common.Content.Components;
using SuperMemoAssistant.SuperMemo.Common.Extensions;
using SuperMemoAssistant.SuperMemo.Hooks;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using SuperMemoAssistant.Sys.SparseClusteredArray;

//using SuperMemoAssistant.SuperMemo.SuperMemo17.Content.Components;
//using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.Common.Content
{
  public class ComponentRegistry : SMHookIOBase, IComponentRegistry
  {
    #region Constants & Statics

    protected const           string ComponFileName          = "compon.dat";
    protected static readonly byte[] CGroupHeader            = { 0x31, 0xD4 };
    protected const           ushort CHtmlHeader             = 7181;
    protected const           ushort CTextHeader             = 8704;
    protected const           ushort CRtfHeader              = 7436;
    protected const           ushort CSpellingHeader         = 8705;
    protected const           ushort CImageHeader            = 6402;
    protected const           ushort CSoundHeader            = 12291;
    protected const           ushort CVideoHeader            = 7940;
    protected const           ushort CShapeEllipseHeader     = 6917;
    protected const           ushort CShapeRectHeader        = 6918;
    protected const           ushort CShapeRoundedRectHeader = 6919;

    protected static readonly IEnumerable<string> TargetFiles = new[]
    {
      ComponFileName,
    };

    #endregion




    #region Properties & Fields - Non-Public

    protected ConcurrentDictionary<int, ComponentGroup> ComponentGroups { get; set; } =
      new ConcurrentDictionary<int, ComponentGroup>();

    protected SparseClusteredArray<byte> CompSCA { get; } = new SparseClusteredArray<byte>();

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public int Count => ComponentGroups.Count;

    /// <inheritdoc />
    public IComponentGroup this[int offset] => ComponentGroups.SafeGet(offset);

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    protected override void Cleanup()
    {
      ComponentGroups.Clear();
      CompSCA.Clear();
    }

    protected override void CommitFromFiles()
    {
      using (Stream cStream = File.OpenRead(Collection.GetInfoFilePath(ComponFileName)))
      {
        var compGroups = ParseCompGroupStream(cStream);

        foreach (var cGroup in compGroups)
          Commit(cGroup);
      }
    }

    protected override void CommitFromMemory()
    {
      foreach (SegmentStream cStream in CompSCA.GetStreams())
      {
        var compGroups = ParseCompGroupStream(cStream);

        foreach (var cGroup in compGroups)
          Commit(cGroup);
      }
    }


    //
    // Hooks-related

    public override IEnumerable<string> GetTargetFilePaths()
    {
      return TargetFiles.Select(f => Collection.GetInfoFilePath(f));
    }

    protected override SparseClusteredArray<byte> GetSCAForFileName(string fileName)
    {
      switch (fileName)
      {
        case ComponFileName:
          return CompSCA;

        default:
          return null;
      }
    }

    public IEnumerable<IComponentGroup> GetAll()
    {
      return ComponentGroups.Values.Select(cg => (IComponentGroup)cg);
    }

    #endregion




    #region Methods

    //
    // Native file parsing

    protected ComponentBase ParseCompStream(BinaryReader binStream)
    {
      ushort typeHeader = binStream.ReadUInt16();

      switch (typeHeader)
      {
        case CHtmlHeader:
          var htmlStruct = binStream.ReadStruct<InfComponentsHtml17>();
          return new ComponentHtml(ref htmlStruct);

        case CTextHeader:
          var textStruct = binStream.ReadStruct<InfComponentsText17>();
          return new ComponentText(ref textStruct);

        case CRtfHeader:
          var rtfStruct = binStream.ReadStruct<InfComponentsRtf17>();
          return new ComponentRtf(ref rtfStruct);

        case CSpellingHeader:
          var spellingStruct = binStream.ReadStruct<InfComponentsSpelling17>();
          return new ComponentSpelling(ref spellingStruct);

        case CImageHeader:
          var imgStruct = binStream.ReadStruct<InfComponentsImage17>();
          return new ComponentImage(ref imgStruct);

        case CSoundHeader:
          var audioStruct = binStream.ReadStruct<InfComponentsSound17>();
          return new ComponentSound(ref audioStruct);

        case CVideoHeader:
          var videoStruct = binStream.ReadStruct<InfComponentsVideo17>();
          return new ComponentVideo(ref videoStruct);

        case CShapeEllipseHeader:
          var shapeEllipseStruct = binStream.ReadStruct<InfComponentsShape17>();
          return new ComponentShapeEllipse(ref shapeEllipseStruct);

        case CShapeRectHeader:
          var shapeRectStruct = binStream.ReadStruct<InfComponentsShape17>();
          return new ComponentShapeRectangle(ref shapeRectStruct);

        case CShapeRoundedRectHeader:
          var shapeRoundedRectStruct = binStream.ReadStruct<InfComponentsShape17>();
          return new ComponentShapeRoundedRectangle(ref shapeRoundedRectStruct);
      }

      return null;
    }

    protected List<ComponentGroup> ParseCompGroupStream(Stream cStream)
    {
      List<ComponentGroup> ret = new List<ComponentGroup>();

      using (BinaryReader binStream = new BinaryReader(cStream,
                                                       Encoding.Default,
                                                       true))
        while (binStream.FindSequence(CGroupHeader))
        {
          // Header
          ushort length    = binStream.ReadUInt16();
          long   position  = binStream.BaseStream.Position;
          int    dummy     = binStream.ReadInt32();
          int    compCount = binStream.ReadChar();
          ushort offset    = binStream.ReadUInt16();

          // Skip unknown data
          binStream.ReadChars(offset);

          ComponentGroup cGroup = new ComponentGroup((int)position - 4);
          ComponentBase  comp;

          while (compCount-- > 0 && (comp = ParseCompStream(binStream)) != null)
            cGroup.AddComponent(comp);

          ret.Add(cGroup);
          binStream.BaseStream.Seek(position + length,
                                    SeekOrigin.Begin);
        }

      return ret;
    }

    protected virtual void Commit(ComponentGroup cGroup)
    {
      var oldCGroup = ComponentGroups.SafeGet(cGroup.Offset);

      if (oldCGroup != null)
      {
        oldCGroup.Update(cGroup);
        try
        {
          OnComponentGroupModified?.Invoke(new SMComponentGroupArgs(Core.SM,
                                                                    cGroup));
        }
        catch (Exception ex)
        {
          LogTo.Error(ex,
                      "Error while signaling ComponentGroup Update");
        }
      }

      else
      {
        ComponentGroups[cGroup.Offset] = cGroup;
        try
        {
          OnComponentGroupCreated?.Invoke(new SMComponentGroupArgs(Core.SM,
                                                                   cGroup));
        }
        catch (Exception ex)
        {
          LogTo.Error(ex,
                      "Error while signaling ComponentGroup Update");
        }
      }
    }

    #endregion




    #region Events

    public event Action<SMComponentGroupArgs> OnComponentGroupCreated;
    public event Action<SMComponentGroupArgs> OnComponentGroupDeleted;
    public event Action<SMComponentGroupArgs> OnComponentGroupModified;

    #endregion
  }
}
