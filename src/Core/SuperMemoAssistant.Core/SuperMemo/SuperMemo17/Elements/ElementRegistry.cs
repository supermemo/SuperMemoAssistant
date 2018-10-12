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
// Created On:   2018/06/01 14:12
// Modified On:  2018/06/07 01:00
// Modified By:  Alexis

#endregion




using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Anotar.Serilog;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.SuperMemo.Hooks;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using SuperMemoAssistant.Sys;
using SuperMemoAssistant.Sys.Collections;
using Task = SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types.Task;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Elements
{
  [InitOnLoad]
  public class ElementRegistry : SMHookIOBase, IElementRegistry
  {
    #region Constants & Statics

    public static ElementRegistry Instance { get; } = new ElementRegistry();

    protected static readonly IEnumerable<string> TargetFiles = new[]
    {
      SMConst.Files.ContentsFileName, SMConst.Files.ElementsInfoFileName,
    };

    #endregion




    #region Properties & Fields - Non-Public

    protected ConcurrentDictionary<int, ElementBase> Elements { get; set; } =
      new ConcurrentDictionary<int, ElementBase>();

    protected SparseClusteredArray<byte> ElementsSCA { get; set; } = new SparseClusteredArray<byte>();
    protected SparseClusteredArray<byte> ContentsSCA { get; set; } = new SparseClusteredArray<byte>();

    #endregion




    #region Constructors

    protected ElementRegistry() { }

    #endregion




    #region Properties Impl - Public

    public IElement Root => (IElement)Elements[1];

    //
    // Elements

    public IElement this[int id] => (IElement)Elements.SafeGet(id);

    public int Count => Root.DescendantCount + 1;

    #endregion




    #region Methods Impl

    //
    // Lifecycle

    /// <inheritdoc />
    protected override void Initialize()
    {
      CommitFromFiles();
    }

    /// <inheritdoc />
    protected override void Cleanup()
    {
      Elements.Values.ForEach(e => e.Dispose());
      Elements.Clear();
      ElementsSCA.Clear();
      ContentsSCA.Clear();
    }

    protected override void CommitFromMemory()
    {
      Dictionary<int, InfContentsElem> cttElems = new Dictionary<int, InfContentsElem>();
      Dictionary<int, InfElementsElem> elElems  = new Dictionary<int, InfElementsElem>();

      foreach (SegmentStream cttStream in ContentsSCA.GetStreams())
        StreamToStruct(
          cttStream,
          InfContentsElem.SizeOfContentsElem,
          cttElems
        );

      foreach (SegmentStream elStream in ElementsSCA.GetStreams())
        StreamToStruct(
          elStream,
          InfElementsElem.SizeOfElementsElem,
          elElems
        );

      foreach (int id in cttElems.Keys.Union(elElems.Keys))
        Commit(id,
               cttElems.SafeGet(id),
               elElems.SafeGet(id));
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
        case SMConst.Files.ContentsFileName:
          return ContentsSCA;

        case SMConst.Files.ElementsInfoFileName:
          return ElementsSCA;

        default:
          return null;
      }
    }


    //
    // UI

    public bool Add(ElementBuilder builder)
    {
      List<IDisposable> toDispose = new List<IDisposable>();

      try
      {
        if (builder.Parent != null)
        {
          toDispose.Add(new HookSnapshot());
          Svc.SMA.UI.ElementWindow.CurrentHookId = builder.Parent.Id;
        }

        if (builder.Concept != null)
        {
          toDispose.Add(new ConceptSnapshot());
          Svc.SMA.UI.ElementWindow.SetCurrentConcept(builder.Concept.Id);
        }

        toDispose.Add(new ClipboardSnapshot());
        Clipboard.SetText(builder.Content);

        switch (builder.Type)
        {
          case ElementType.Topic:
            Svc.SMA.UI.ElementWindow.PasteArticle();
            break;

          default:
            throw new NotImplementedException();
        }

        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
      finally
      {
        toDispose.ForEach(d => d.Dispose());
      }
    }

    public bool Delete(IElement element)
    {
      throw new NotImplementedException();
    }


    //
    // Enumerable

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc />
    public IEnumerator<IElement> GetEnumerator()
    {
      return Elements.Values.Select(e => (IElement)e).GetEnumerator();
    }

    #endregion




    #region Methods

    protected virtual ElementBase CreateInternal(int             id,
                                                 InfContentsElem cttElem,
                                                 InfElementsElem elElem)
    {
      switch ((ElementType)elElem.elementType)
      {
        case ElementType.Topic:
          return new Topic(id,
                           cttElem,
                           elElem);

        case ElementType.Item:
          return new Item(id,
                          cttElem,
                          elElem);

        case ElementType.ConceptGroup:
          return new ConceptGroup(id,
                                  cttElem,
                                  elElem);

        case ElementType.Task:
          return new Task(id,
                          cttElem,
                          elElem);
      }

#if DEBUG
      throw new InvalidDataException("Unknown object type");
#else
      return new Topic(id, cttElem, elElem);
#endif
    }

    protected virtual void CommitFromFiles()
    {
      using (Stream cttStream = File.OpenRead(Collection.GetInfoFilePath(SMConst.Files.ContentsFileName)))
      using (Stream elStream = File.OpenRead(Collection.GetInfoFilePath(SMConst.Files.ElementsInfoFileName)))
      {
        var cttElems = StreamToStruct<InfContentsElem>(
          cttStream,
          InfContentsElem.SizeOfContentsElem
        );

        var elElems = StreamToStruct<InfElementsElem>(
          elStream,
          InfElementsElem.SizeOfElementsElem
        );

        foreach (int id in cttElems.Keys.Union(elElems.Keys))
          Commit(id,
                 cttElems.SafeGet(id),
                 elElems.SafeGet(id));
      }
    }

    protected virtual void Commit(int             id,
                                  InfContentsElem cttElem,
                                  InfElementsElem elElem)
    {
      var el = Elements.SafeGet(id);

      if (el != null)
      {
        var flags = el.Update(cttElem,
                              elElem);

        if (flags.HasFlag(ElementFieldFlags.Deleted))
          try
          {
            OnElementDeleted?.Invoke(new SMElementArgs(SMA.Instance,
                                                       (IElement)el));
          }
          catch (Exception ex)
          {
            LogTo.Error(ex,
                        "Error while signaling Element Deleted event");
          }

        else
          try
          {
            OnElementModified?.Invoke(new SMElementChangedArgs(SMA.Instance,
                                                               (IElement)el,
                                                               flags));
          }
          catch (Exception ex)
          {
            LogTo.Error(ex,
                        "Error while signaling Element Modified event");
          }
      }

      else
      {
        el = CreateInternal(id,
                            cttElem,
                            elElem);
        Elements[id] = el;

        try
        {
          OnElementCreated?.Invoke(new SMElementArgs(SMA.Instance,
                                                     (IElement)el));
        }
        catch (Exception ex)
        {
          LogTo.Error(ex,
                      "Error while signaling Element Created event");
        }
      }
    }

    #endregion




    #region Events

    public event Action<SMElementArgs>        OnElementCreated;
    public event Action<SMElementArgs>        OnElementDeleted;
    public event Action<SMElementChangedArgs> OnElementModified;

    #endregion
  }
}
