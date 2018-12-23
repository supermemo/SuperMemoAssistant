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
// Modified On:  2018/12/09 15:59
// Modified By:  Alexis

#endregion




using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.SuperMemo.Hooks;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using SuperMemoAssistant.SuperMemo.SuperMemo17.UI.Element;
using SuperMemoAssistant.Sys;
using SuperMemoAssistant.Sys.Collections;
using SuperMemoAssistant.Sys.UIAutomation;

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

    private Mutex AddMutex { get; } = new Mutex();

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

      foreach (int id in cttElems.Keys)
        Commit(id,
               cttElems.SafeGet(id),
               null);

      foreach (int id in elElems.Keys)
        Commit(id,
               null,
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
    // 

    public bool Add(ElementBuilder builder)
    {
      bool              success             = false;
      bool              inSMUpdateLockMode  = false;
      bool              inSMAUpdateLockMode = false;
      List<IDisposable> toDispose           = new List<IDisposable>();

      try
      {
        int lastElementId = -1;

        //
        // Enter critical section

        AddMutex.WaitOne();

        //
        // Suspend element changed monitoring

        inSMAUpdateLockMode = ElementWdw.Instance.EnterSMAUpdateLock();

        //
        // Freeze element window if we want to insert the element without displaying it immediatly

        if (builder.ShouldDisplay == false)
        {
          inSMUpdateLockMode = ElementWdw.Instance.EnterSMUpdateLock(); // TODO: Pass in EnterUpdateLock

          lastElementId = ElementWdw.Instance.CurrentElementId;
        }

        //
        // Has a parent been specified for the new element ?

        if (builder.Parent != null)
        {
          toDispose.Add(new HookSnapshot());
          ElementWdw.Instance.CurrentHookId = builder.Parent.Id;
        }

        //
        // Has a concept been specified for the new element ?

        if (builder.Concept != null)
        {
          toDispose.Add(new ConceptSnapshot());
          ElementWdw.Instance.SetCurrentConcept(builder.Concept.Id);
        }

        //
        // Focus

        toDispose.Add(new FocusSnapshot()); // TODO: Only if inserting 1 element

        //
        // Select appropriate insertion method, depending on element type and content

        ElementCreationMethod creationMethod;

        switch (builder.ContentType)
        {
#if false
        case ElementBuilder.ContentTypeEnum.RawText:
          creationMethod = ElementCreationMethod.ClipboardContent;

          if (!(builder.Contents[0] is ElementBuilder.TextContent rawTextContent))
            throw new InvalidCastException("ContentTypeEnum.RawText contained a non-text IContent");

          Clipboard.SetText(rawTextContent.Text,
                            rawTextContent.Encoding.EncodingName == Encoding.Unicode.EncodingName
                              ? TextDataFormat.UnicodeText
                              : TextDataFormat.Text);
          break;

        case ElementBuilder.ContentTypeEnum.Html:
          creationMethod = ElementCreationMethod.ClipboardContent;

          if (!(builder.Contents[0] is ElementBuilder.TextContent htmlTextContent))
            throw new InvalidCastException("ContentTypeEnum.RawText contained a non-text IContent");

          ClipboardHelper.CopyToClipboard(htmlTextContent.Text,
                                          htmlTextContent.Text);
          break;
#endif
          // TODO: Handle multiple content

          case ElementBuilder.ContentTypeEnum.RawText:
          case ElementBuilder.ContentTypeEnum.Html:
            creationMethod = ElementCreationMethod.AddElement;

            if (!(builder.Contents[0] is ElementBuilder.TextContent))
              throw new InvalidCastException("ContentTypeEnum.RawText contained a non-text IContent");

            break;


          case ElementBuilder.ContentTypeEnum.Image:
            creationMethod = ElementCreationMethod.AddElement;
            
            if (!(builder.Contents[0] is ElementBuilder.ImageContent))
              throw new InvalidCastException("ContentTypeEnum.Image contained a non-image IContent");

            break;


          default:
            throw new NotImplementedException();
        }

        //
        // Insert the element

        switch (creationMethod)
        {
          case ElementCreationMethod.ClipboardContent:
            if (builder.Type != ElementType.Topic)
              throw new InvalidOperationException("ElementCreationMethod.ClipboardContent can only create Topics.");

            success = ElementWdw.Instance.PasteArticle();

            break;

          case ElementCreationMethod.ClipboardElement:
            success = ElementWdw.Instance.PasteElement();

            break;

          case ElementCreationMethod.AddElement:
            //if (builder.Parent != null && ElementWdw.Instance.CurrentElementId != builder.Parent.Id)
            //{
            //  ElementWdw.Instance.IgnoreElementChange += 1;

            //  ElementWdw.Instance.GoToElement(builder.Parent.Id);
            //}

            string elementDesc = ElementClipboardBuilder.FromElementBuilder(builder);

            success = ElementWdw.Instance.AppendAndAddElementFromText(builder.Type, elementDesc) > 0;

            break;
        }

        //
        // Display original element, and unfreeze window -- or simply resume element changed monitoring

        if (builder.ShouldDisplay == false)
        {
          inSMUpdateLockMode = ElementWdw.Instance.QuitSMUpdateLock() == false;

          if (success)
            ElementWdw.Instance.GoToElement(lastElementId);

          inSMAUpdateLockMode = ElementWdw.Instance.QuitSMAUpdateLock(true);
        }

        else
        {
          inSMAUpdateLockMode = ElementWdw.Instance.QuitSMAUpdateLock();
        }

        return success;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "An exception was thrown while creating a new element in SM.");
        return false;
      }
      finally
      {
        //
        // Unlock SM if necessary

        if (inSMUpdateLockMode)
          try
          {
            ElementWdw.Instance.QuitSMUpdateLock();
          }
          catch (Exception ex)
          {
            LogTo.Error(ex,
                        "Failed to exit SM Update Lock.");
            MessageBox.Show($@"Failed to exit SuperMemo update lock.
You might have to restart SuperMemo.

Exception: {ex}",
                            "Critical error");
          }

        //
        // Restore initial context

        toDispose.ForEach(d =>
        {
          try
          {
            d.Dispose();
          }
          catch (Exception ex)
          {
            LogTo.Error(ex,
                        "Failed to restore context after creating a new SM element.");
            MessageBox.Show($@"Failed to restore initial context.

Exception: {ex}",
                            "Warning");
          }
        });

        //
        // Unlock element changed monitoring if necessary

        if (inSMAUpdateLockMode)
          ElementWdw.Instance.QuitSMAUpdateLock();

        //
        // Exit Critical section

        AddMutex.ReleaseMutex();
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

    public IEnumerable<IElement> FindByName(Regex regex)
    {
      return Elements.Values.Where(e => regex.IsMatch(e.Title)).Select(e => (IElement)e).ToList();
    }

    public IElement FirstOrDefaultByName(Regex regex)
    {
      return (IElement)Elements.Values.FirstOrDefault(e => regex.IsMatch(e.Title));
    }

    public IElement FirstOrDefaultByName(string exactName)
    {
      return (IElement)Elements.Values.FirstOrDefault(e => e.Title == exactName);
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

    protected virtual void Commit(int              id,
                                  InfContentsElem? cttElem,
                                  InfElementsElem? elElem)
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
                            cttElem ?? default(InfContentsElem),
                            elElem ?? default(InfElementsElem));
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

    protected void DelayedFocus(ref List<IDisposable> toDispose) { }

    #endregion




    #region Events

    public event Action<SMElementArgs>        OnElementCreated;
    public event Action<SMElementArgs>        OnElementDeleted;
    public event Action<SMElementChangedArgs> OnElementModified;

    #endregion




    #region Enums

    private enum ElementCreationMethod
    {
      ClipboardContent,
      ClipboardElement,
      AddElement,
    }

    #endregion
  }
}
