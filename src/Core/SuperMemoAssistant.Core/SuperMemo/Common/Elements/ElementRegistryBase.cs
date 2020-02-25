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
// Modified On:  2020/02/22 16:33
// Modified By:  Alexis

#endregion




using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Anotar.Serilog;
using MoreLinq;
using Nito.AsyncEx;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SuperMemo.Common.Elements.Builders;
using SuperMemoAssistant.SuperMemo.Common.Extensions;
using SuperMemoAssistant.SuperMemo.Hooks;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types;
using SuperMemoAssistant.Sys.SparseClusteredArray;

namespace SuperMemoAssistant.SuperMemo.Common.Elements
{
  public abstract class ElementRegistryBase
    : SMHookIOBase,
      IElementRegistry
  {
    #region Constants & Statics

    private const int WaitForElementAnyTriggerId     = int.MinValue;
    private const int WaitForElementUpdatedTriggerId = int.MinValue;
    private const int WaitForElementCreatedTriggerId = int.MaxValue;

    #endregion




    #region Properties & Fields - Non-Public

    //
    // Sync

    private readonly Mutex _addMutex = new Mutex();

    private readonly AsyncManualResetEvent _waitForElementCreatedEvent = new AsyncManualResetEvent();
    private readonly AsyncManualResetEvent _waitForElementUpdatedEvent = new AsyncManualResetEvent();
    private readonly AsyncManualResetEvent _waitForElementAnyEvent     = new AsyncManualResetEvent();
    private readonly ManualResetEventSlim  _waitForElementIdEvent      = new ManualResetEventSlim();

    private int _waitForElementId       = -1;
    private int _waitForElementResultId = -1;


    //
    // Core

    protected ConcurrentDictionary<int, ElementBase> Elements { get; } = new ConcurrentDictionary<int, ElementBase>();

    protected SparseClusteredArray<byte> ElementsSCA { get; } = new SparseClusteredArray<byte>();
    protected SparseClusteredArray<byte> ContentsSCA { get; } = new SparseClusteredArray<byte>();


    //
    // Hooks-related

    protected IEnumerable<string> TargetFiles { get; }


    //
    // Inheritance

    protected abstract IElementRegistryUpdater Updater { get; }

    #endregion




    #region Constructors

    /// <inheritdoc />
    protected ElementRegistryBase()
    {
      TargetFiles = new[]
      {
        Collection.GetInfoFilePath(SMConst.Files.ContentsFileName),
        Collection.GetInfoFilePath(SMConst.Files.ElementsInfoFileName),
      };
    }

    #endregion




    #region Properties & Fields - Public

    public int LastCreatedElementId { get; private set; }
    public int LastUpdatedElementId { get; private set; }
    public int LastElementId        { get; private set; }

    #endregion




    #region Properties Impl - Public

    public IElement Root => (IElement)Elements[1];

    public IElement this[int id] => Elements.SafeGet(id);

    public int Count => Root.DescendantCount + 1;

    #endregion




    #region Methods Impl

    //
    // Lifecycle

    protected override void Cleanup()
    {
      IEnumerableEx.ForEach(Elements.Values, e => e.Dispose());

      Elements.Clear();
      ElementsSCA.Clear();
      ContentsSCA.Clear();
    }

    protected override void CommitFromMemory()
    {
      Updater.CommitFromMemory(ContentsSCA, ElementsSCA);
    }

    protected override void CommitFromFiles()
    {
      Updater.CommitFromFiles(Collection);
    }


    //
    // Hooks-related

    public override IEnumerable<string> GetTargetFilePaths()
    {
      return TargetFiles;
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
    // Core

    public bool Delete(IElement element)
    {
      throw new NotImplementedException();
    }

    public bool Add(out List<ElemCreationResult> results,
                    ElemCreationFlags            options,
                    params ElementBuilder[]      builders)
    {
      if (builders == null || builders.Length == 0)
      {
        results = new List<ElemCreationResult>();
        return false;
      }

      var inSMUpdateLockMode  = false;
      var inSMAUpdateLockMode = false;
      var singleMode          = builders.Length == 1;

      results = new List<ElemCreationResult>(
        builders.Select(
          b => new ElemCreationResult(ElemCreationResultCode.ErrorUnknown, b))
      );

      try
      {
        bool success          = true;
        int  restoreElementId = Core.SM.UI.ElementWdw.CurrentElementId;
        int  restoreHookId    = Core.SM.UI.ElementWdw.CurrentHookId;

        //
        // Enter critical section

        _addMutex.WaitOne();

        //
        // Suspend element changed monitoring

        inSMAUpdateLockMode = Core.SM.UI.ElementWdw.EnterSMAUpdateLock();

        //
        // Save states

        //toDispose.Add(new HookSnapshot());
        //toDispose.Add(new ConceptSnapshot());

        //
        // Focus

        // toDispose.Add(new FocusSnapshot(true)); // TODO: Only if inserting 1 element

        //
        // Freeze element window if we want to insert the element without displaying it immediatly

        if (singleMode == false || builders[0].ShouldDisplay == false)
          inSMUpdateLockMode = Core.SM.UI.ElementWdw.EnterSMUpdateLock(); // TODO: Pass in EnterUpdateLock

        foreach (var result in results)
        {
          List<IDisposable> toDispose = new List<IDisposable>();

          try
          {
            toDispose.Add(new ConceptSnapshot());
            toDispose.Add(new HookSnapshot());

            result.Result    = AddElement(result.Builder, options, restoreHookId, out int elemId);
            result.ElementId = elemId;

            success = success && result.Success;
          }
          finally
          {
            //
            // Restore initial context
            foreach (var d in toDispose)
              try
              {
                d.Dispose();
              }
              catch (Exception ex)
              {
                LogTo.Warning(ex, "Failed to restore context after creating a new SM element.");
                MessageBox.Show($@"Failed to restore initial context after creating a new SM element.
Your hook and/or current concept might have been changed.

Exception: {ex}",
                                "Warning");
              }
          }
        }

        //
        // Display original element, and unfreeze window -- or simply resume element changed monitoring

        if (inSMUpdateLockMode)
        {
          inSMUpdateLockMode = Core.SM.UI.ElementWdw.QuitSMUpdateLock() == false;

          Core.SM.UI.ElementWdw.GoToElement(restoreElementId);

          inSMAUpdateLockMode = Core.SM.UI.ElementWdw.QuitSMAUpdateLock(true);
        }

        else
        {
          inSMAUpdateLockMode = Core.SM.UI.ElementWdw.QuitSMAUpdateLock();
        }

        return true;
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
            Core.SM.UI.ElementWdw.QuitSMUpdateLock();
          }
          catch (Exception ex)
          {
            LogTo.Warning(ex, "Failed to exit SM Update Lock.");
            MessageBox.Show($@"Failed to exit SuperMemo UI update lock.
You might have to restart SuperMemo.

Exception: {ex}",
                            "Critical error");
          }

        //
        // Unlock element changed monitoring if necessary

        if (inSMAUpdateLockMode)
          Core.SM.UI.ElementWdw.QuitSMAUpdateLock();

        //
        // Exit Critical section

        _addMutex.ReleaseMutex();
      }
    }


    //
    // Enumerable

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

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

    //
    // 

    private bool CanAddElement(IElement          parent,
                               ElemCreationFlags options)
    {
      if (options.HasFlag(ElemCreationFlags.ForceCreate))
        return true;

      return parent.ChildrenCount < Core.SM.UI.ElementWdw.LimitChildrenCount;
    }

    private int CreateAutoSubfolders(IElement parent,
                                     int      subFolderNo)
    {
      string title = $"[{subFolderNo}] {parent.Title}";

      AddElement(
        new ElementBuilder(ElementType.Topic)
          .WithParent(parent)
          .WithTitle(title)
          .WithStatus(ElementStatus.Dismissed)
          .WithPriority(100.0)
          .WithConcept(parent.Concept)
          .DoNotDisplay(),
        ElemCreationFlags.ForceCreate,
        parent.Id,
        out int elemId
      );

      if (WaitForElement(elemId, title) == false)
        return -1;

      return elemId;
    }

    private int FindDestinationBranch(IElement          parent,
                                      ElemCreationFlags options)
    {
      if (options.HasFlag(ElemCreationFlags.CreateSubfolders) == false)
        return CanAddElement(parent, options) ? parent.Id : -1;

      var regExSubfolders = new Regex($"\\[([0-9]+)\\] {Regex.Escape(parent.Title)}");

      var subFolders = parent.Children
                             .Where(child => child.Deleted == false && child.Title != null)
                             .Select(child => (child, regExSubfolders.Match(child.Title)))
                             .Where(p => p.Item2.Success)
                             .ToList();

      if (subFolders.Any() == false)
        return CreateAutoSubfolders(parent, 1);

      var subFolder = subFolders.MaxBy(p => int.Parse(p.Item2.Groups[1].Value))
                                .First();

      if (subFolder.child == null || CanAddElement(subFolder.child, ElemCreationFlags.None) == false)
        return CreateAutoSubfolders(
          parent,
          subFolder.child == null
            ? 1
            : int.Parse(subFolder.Item2.Groups[1].Value) + 1
        );

      return subFolder.child.Id;
    }

    private bool AdjustRoot(int parentId)
    {
      var  parentEl      = this[parentId];
      var  currentRootId = Core.SM.UI.ElementWdw.CurrentRootId;
      bool needAdjust    = true;

      while (parentEl.Id != currentRootId && parentEl.Id != 1 && parentEl.Parent != null)
        parentEl = parentEl.Parent;

      if (parentEl.Id == currentRootId)
        needAdjust = false;

      // Revert to root concept
      if (needAdjust)
        Core.SM.UI.ElementWdw.SetCurrentConcept(1);

      return needAdjust;
    }

    private bool AddElement(ElementBuilder builder, out int elemId)
    {
      elemId = -1;

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

          return Core.SM.UI.ElementWdw.PasteArticle();

        case ElementCreationMethod.ClipboardElement:
          return Core.SM.UI.ElementWdw.PasteElement();

        case ElementCreationMethod.AddElement:
          elemId = Core.SM.UI.ElementWdw.AppendAndAddElementFromText(
            builder.Type,
            builder.ToElementString());

          return elemId > 0;

        default:
          throw new NotImplementedException();
      }
    }

    private ElemCreationResultCode AddElement(ElementBuilder    builder,
                                              ElemCreationFlags options,
                                              int               originalHookId,
                                              out int           elemId)
    {
      var successFlag = ElemCreationResultCode.Success;
      elemId = -1;

      try
      {
        //
        // Has a parent been specified for the new element ?

        var parentId = builder.Parent?.Id ?? originalHookId;

        //
        // Create or use auto subfolder, if requested

        parentId = FindDestinationBranch(this[parentId], options);

        if (parentId <= 0 || this[parentId] == null)
          return ElemCreationResultCode.ErrorTooManyChildren;

        //
        // Has a concept been specified for the new element ?

        if (builder.Concept != null)
          if (Core.SM.UI.ElementWdw.SetCurrentConcept(builder.Concept.Id) == false)
            successFlag |= ElemCreationResultCode.WarningConceptNotSet;

        //
        // Make sure concept & root are valid

        if (AdjustRoot(parentId))
          successFlag |= ElemCreationResultCode.WarningConceptNotSet;

        //
        // Set hook *after* adjusting concept

        Core.SM.UI.ElementWdw.CurrentHookId = parentId;

        //
        // Create

        return AddElement(builder, out elemId)
          ? successFlag
          : ElemCreationResultCode.ErrorUnknown;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception caught while adding new element");
        return ElemCreationResultCode.ErrorUnknown;
      }
    }


    //
    // Internal registry works

    protected ElementBase GetInternal(int id) => Elements.SafeGet(id);

    protected void OnElementCreatedInternal(ElementBase el)
    {
      try
      {
        OnElementCreated?.Invoke(new SMElementArgs(Core.SM, el));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Error while signaling Element Created event");
      }
      finally
      {
        LastElementId = LastCreatedElementId = el.Id;

        if (_waitForElementId == WaitForElementCreatedTriggerId || _waitForElementId == WaitForElementAnyTriggerId)
        {
          _waitForElementId       = -1;
          _waitForElementResultId = el.Id;

          _waitForElementCreatedEvent.Set();
        }

        else if (el.Id == _waitForElementId)
        {
          _waitForElementId = -1;

          _waitForElementIdEvent.Set();
        }

        _waitForElementAnyEvent.Set();
      }
    }

    protected void OnElementUpdatedInternal(ElementBase el, ElementFieldFlags flags)
    {
      bool deleted = flags.HasFlag(ElementFieldFlags.Deleted);

      try
      {
        if (deleted)
          OnElementDeleted?.Invoke(new SMElementArgs(Core.SM, el));

        else
          OnElementModified?.Invoke(new SMElementChangedArgs(Core.SM, el, flags));
      }
      catch (Exception ex)
      {
        var eventType = deleted ? "Deleted" : "Modified";

        LogTo.Error(ex, $"Error while signaling Element {eventType} event");
      }
      finally
      {
        LastElementId = el.Id;

        if (_waitForElementId == WaitForElementCreatedTriggerId && flags.HasFlag(ElementFieldFlags.Deleted) && el.Deleted == false)
        {
          LastCreatedElementId    = el.Id;
          _waitForElementId       = -1;
          _waitForElementResultId = el.Id;

          _waitForElementCreatedEvent.Set();
        }

        else if (_waitForElementId == WaitForElementUpdatedTriggerId || _waitForElementId == WaitForElementAnyTriggerId)
        {
          LastUpdatedElementId    = el.Id;
          _waitForElementId       = -1;
          _waitForElementResultId = el.Id;

          _waitForElementUpdatedEvent.Set();
        }

        else if (el.Id == _waitForElementId)
        {
          _waitForElementId = -1;

          _waitForElementIdEvent.Set();
        }

        _waitForElementAnyEvent.Set();
      }
    }

    public Task<int> WaitForNextElement(int timeOutMs = 3000)
    {
      using (var cts = new CancellationTokenSource(timeOutMs))
        return WaitForNextElement(cts.Token);
    }

    public async Task<int> WaitForNextElement(CancellationToken ct)
    {
      try
      {
        _waitForElementId = int.MinValue;
        _waitForElementAnyEvent.Reset();

        await _waitForElementAnyEvent.WaitAsync(ct);

        return ct.IsCancellationRequested ? -1 : _waitForElementResultId;
      }
      finally
      {
        _waitForElementId = -1;
      }
    }

    public Task<int> WaitForNextCreatedElement(int timeOutMs = 3000)
    {
      using (var cts = new CancellationTokenSource(timeOutMs))
        return WaitForNextCreatedElement(cts.Token);
    }

    public async Task<int> WaitForNextCreatedElement(CancellationToken ct)
    {
      try
      {
        _waitForElementId = int.MaxValue;
        _waitForElementCreatedEvent.Reset();

        await _waitForElementCreatedEvent.WaitAsync(ct);

        return ct.IsCancellationRequested ? -1 : _waitForElementResultId;
      }
      finally
      {
        _waitForElementId = -1;
      }
    }

    public Task<int> WaitForNextUpdatedElement(int timeOutMs = 3000)
    {
      using (var cts = new CancellationTokenSource(timeOutMs))
        return WaitForNextUpdatedElement(cts.Token);
    }

    public async Task<int> WaitForNextUpdatedElement(CancellationToken ct)
    {
      try
      {
        _waitForElementId = int.MinValue;
        _waitForElementUpdatedEvent.Reset();

        await _waitForElementUpdatedEvent.WaitAsync(ct);

        return ct.IsCancellationRequested ? -1 : _waitForElementResultId;
      }
      finally
      {
        _waitForElementId = -1;
      }
    }

    // TODO: Make this thread-safe
    // TODO: Make this async
    private bool WaitForElement(int elemId, string title)
    {
      try
      {
        _waitForElementIdEvent.Reset();
        _waitForElementId = elemId;

        var elem = this[elemId];

        if (elem != null && elem.Title == title)
          return true;

        return _waitForElementIdEvent.Wait(3000);
      }
      finally
      {
        _waitForElementId = -1;
      }
    }

    #endregion




    #region Methods Abs

    protected abstract ElementBase CreateInternal(
      int         id,
      ElementType elementType);

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
