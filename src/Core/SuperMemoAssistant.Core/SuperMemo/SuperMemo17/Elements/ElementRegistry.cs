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
// Created On:   2019/03/02 18:29
// Modified On:  2019/04/12 03:57
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
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.SuperMemo.Hooks;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Builders;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using SuperMemoAssistant.SuperMemo.SuperMemo17.UI.Element;
using SuperMemoAssistant.Sys.SparseClusteredArray;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Elements
{
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

    public IElement this[int id] => Elements.SafeGet(id);

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
      Dictionary<int, InfContentsElem>          cttElems = new Dictionary<int, InfContentsElem>();
      Dictionary<int, InfElementsElemContainer> elElems  = new Dictionary<int, InfElementsElemContainer>();

      foreach (SegmentStream cttStream in ContentsSCA.GetStreams())
        StreamToStruct<InfContentsElem, InfContentsElem>(
          cttStream,
          InfContentsElem.SizeOfContentsElem,
          e => e,
          cttElems
        );

      foreach (SegmentStream elStream in ElementsSCA.GetStreams())
        StreamToStruct<InfElementsElemContainer, InfElementsElem>(
          elStream,
          InfElementsElem.SizeOfElementsElem,
          e => new InfElementsElemContainer(e),
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

    public HashSet<ElementBuilder> Add(params ElementBuilder[] builders)
    {
      if (builders == null || builders.Length == 0)
        return new HashSet<ElementBuilder>();

      bool                    inSMUpdateLockMode  = false;
      bool                    inSMAUpdateLockMode = false;
      bool                    singleMode          = builders.Length == 1;
      List<IDisposable>       toDispose           = new List<IDisposable>();
      HashSet<ElementBuilder> failedBuilders      = new HashSet<ElementBuilder>(builders);

      try
      {
        int restoreElementId = ElementWdw.Instance.CurrentElementId;
        int restoreHookId = ElementWdw.Instance.CurrentHookId;

        //
        // Enter critical section

        AddMutex.WaitOne();

        //
        // Suspend element changed monitoring

        inSMAUpdateLockMode = ElementWdw.Instance.EnterSMAUpdateLock();

        //
        // Save states

        toDispose.Add(new HookSnapshot());
        //toDispose.Add(new ConceptSnapshot());

        //
        // Freeze element window if we want to insert the element without displaying it immediatly

        if (singleMode == false || builders[0].ShouldDisplay == false)
          inSMUpdateLockMode = ElementWdw.Instance.EnterSMUpdateLock(); // TODO: Pass in EnterUpdateLock

        foreach (var builder in builders)
        {
          bool success = false;

          //
          // Has a parent been specified for the new element ?

          ElementWdw.Instance.CurrentHookId = builder.Parent?.Id ?? restoreHookId;

          //
          // Has a concept been specified for the new element ?

          //if (builder.Concept != null)
            //ElementWdw.Instance.SetCurrentConcept(builder.Concept.Id);

          //
          // Focus

          // toDispose.Add(new FocusSnapshot(true)); // TODO: Only if inserting 1 element

          //
          // Select appropriate insertion method, depending on element type and content

          var creationMethod = ElementCreationMethod.AddElement;

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
              string elementDesc = builder.ToElementString();
              success = ElementWdw.Instance.AppendAndAddElementFromText(builder.Type, elementDesc) > 0;

              break;
          }

          if (success)
            failedBuilders.Remove(builder);
        }

        //
        // Display original element, and unfreeze window -- or simply resume element changed monitoring

        if (inSMUpdateLockMode)
        {
          inSMUpdateLockMode = ElementWdw.Instance.QuitSMUpdateLock() == false;
          
          ElementWdw.Instance.GoToElement(restoreElementId);

          inSMAUpdateLockMode = ElementWdw.Instance.QuitSMAUpdateLock(true);
        }

        else
        {
          inSMAUpdateLockMode = ElementWdw.Instance.QuitSMAUpdateLock();
        }

        return failedBuilders;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "An exception was thrown while creating a new element in SM.");
        return failedBuilders;
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

    protected virtual ElementBase CreateInternal(int                      id,
                                                 InfContentsElem          cttElem,
                                                 InfElementsElemContainer elElem)
    {
      switch ((ElementType)elElem._elem.elementType)
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
        var cttElems = StreamToStruct<InfContentsElem, InfContentsElem>(
          cttStream,
          InfContentsElem.SizeOfContentsElem,
          s => s
        );

        var elElems = StreamToStruct<InfElementsElemContainer, InfElementsElem>(
          elStream,
          InfElementsElem.SizeOfElementsElem,
          e => new InfElementsElemContainer(e)
        );

        foreach (int id in cttElems.Keys.Union(elElems.Keys))
          Commit(id,
                 cttElems.SafeGet(id),
                 elElems.SafeGet(id));
      }
    }

    protected virtual void Commit(int                      id,
                                  InfContentsElem          cttElem,
                                  InfElementsElemContainer elElem)
    {
      var el = Elements.SafeGet(id);

      if (el != null)
      {
        var flags = el.Update(cttElem,
                              elElem);

        if (flags.HasFlag(ElementFieldFlags.Deleted))
          try
          {
            OnElementDeleted?.Invoke(new SMElementArgs(SMA.SMA.Instance,
                                                       el));
          }
          catch (Exception ex)
          {
            LogTo.Error(ex,
                        "Error while signaling Element Deleted event");
          }

        else
          try
          {
            OnElementModified?.Invoke(new SMElementChangedArgs(SMA.SMA.Instance,
                                                               el,
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
          OnElementCreated?.Invoke(new SMElementArgs(SMA.SMA.Instance,
                                                     el));
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
