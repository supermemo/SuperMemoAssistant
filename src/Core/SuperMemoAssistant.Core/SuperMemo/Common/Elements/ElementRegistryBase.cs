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




namespace SuperMemoAssistant.SuperMemo.Common.Elements
{
  using System;
  using System.Collections;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.Globalization;
  using System.Linq;
  using System.Text.RegularExpressions;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows;
  using Anotar.Serilog;
  using Builders;
  using Extensions;
  using Hooks;
  using Interop;
  using Interop.SuperMemo.Core;
  using Interop.SuperMemo.Elements;
  using Interop.SuperMemo.Elements.Builders;
  using Interop.SuperMemo.Elements.Models;
  using Interop.SuperMemo.Elements.Types;
  using MoreLinq;
  using Nito.AsyncEx;
  using SMA;
  using SuperMemo17.Elements.Types;
  using SuperMemoAssistant.Extensions;
  using Sys.Remoting;
  using Sys.SparseClusteredArray;
  using SysTask = System.Threading.Tasks.Task;

  [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "<Pending>")]
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

    private readonly Mutex                 _addMutex               = new Mutex();
    private readonly AsyncManualResetEvent _waitForElementAnyEvent = new AsyncManualResetEvent();

    private readonly AsyncManualResetEvent _waitForElementCreatedEvent = new AsyncManualResetEvent();
    private readonly AsyncManualResetEvent _waitForElementIdEvent      = new AsyncManualResetEvent();
    private readonly AsyncManualResetEvent _waitForElementUpdatedEvent = new AsyncManualResetEvent();

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
      results = AsyncContext.Run(() => AddInternalAsync(options, builders));

      return results != null;
    }

    // This is a fake async until the new hook engine system is implemented.
    // TODO: New hook engine system https://github.com/supermemo/SuperMemoAssistant/tree/improved-hook-engine
    public RemoteTask<List<ElemCreationResult>> AddAsync(
      ElemCreationFlags       options,
      params ElementBuilder[] builders)
    {
      return AddInternalAsync(options, builders);
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

    public IEnumerable<IElement> FindByComment(Regex regex)
    {
      return Elements.Values.Where(e => regex.IsMatch(e.Comment)).Select(e => (IElement)e).ToList();
    }

    public IElement FirstOrDefaultByName(Regex regex)
    {
      return (IElement)Elements.Values.FirstOrDefault(e => regex.IsMatch(e.Title));
    }

    public IElement FirstOrDefaultByComment(Regex regex)
    {
      return (IElement)Elements.Values.FirstOrDefault(e => e.Comment != null && regex.IsMatch(e.Comment));
    }

    public IElement FirstOrDefaultByName(string exactName)
    {
      return (IElement)Elements.Values.FirstOrDefault(e => e.Title == exactName);
    }

    #endregion




    #region Methods

    /*
    private async Task<int> CreateAutoSubfoldersAsync(
      IElement parent,
      int      subFolderNo)
    {
      var title = $"[{subFolderNo}] {parent.Title}";

      AddElement(
        new ElementBuilder(ElementType.Topic)
          .WithParent(parent)
          .WithTitle(title)
          .WithStatus(ElementStatus.Dismissed)
          .WithPriority(100.0)
          .WithConcept(parent.Concept)
          .DoNotDisplay(),
        ElemCreationFlags.None,
        parent.Id,
        out int elemId
      );

      if (await WaitForElementAsync(elemId, title).ConfigureAwait(false) == false)
        return -1;

      return elemId;
    }

    private IDestinationBranchFinder GetNextDestinationBranchFunc(
      IElement          parent,
      ElemCreationFlags options,
      int               newElemCount)
    {
      // Always add in parent (or cancel if error)
      if (options.HasFlag(ElemCreationFlags.CreateSubfolders) == false)
        return new ConstantBranchFinder(parent);


      if (subFolders.Any() == false)
        return CreateAutoSubfoldersAsync(parent, 1);

      var subFolder = subFolders.MaxBy()
                                .First();

      if (subFolder.child == null || CanAddElement(subFolder.child, ElemCreationFlags.None) == false)
        return CreateAutoSubfolders(
          parent,
          subFolder.child == null
            ? 1
            : int.Parse(subFolder.Item2.Groups[1].Value, CultureInfo.InvariantCulture) + 1
        );

      return subFolder.child.Id;
    }

    private int FindDestinationBranch(IElement          parent,
                                      ElemCreationFlags options)
    {
      var regExSubfolders = new Regex($"\\[([0-9]+)\\] {Regex.Escape(parent.Title)}");

      var subFolders = parent.Children
                             .Where(child => child.Deleted == false && child.Title != null)
                             .Select(child => (child, regExSubfolders.Match(child.Title)))
                             .Where(p => p.Item2.Success)
                             .ToList();

      if (subFolders.Any() == false)
        return CreateAutoSubfoldersAsync(parent, 1);

      var subFolder = subFolders.MaxBy(p => int.Parse(p.Item2.Groups[1].Value, CultureInfo.InvariantCulture))
                                .First();

      if (subFolder.child == null || CanAddElement(subFolder.child, ElemCreationFlags.None) == false)
        return CreateAutoSubfoldersAsync(
          parent,
          subFolder.child == null
            ? 1
            : int.Parse(subFolder.Item2.Groups[1].Value, CultureInfo.InvariantCulture) + 1
        );

      return subFolder.child.Id;
    }
    */

    /// <summary>
    ///   Ensures the <see cref="Core.SM.UI.ElementWdw.CurrentRootId" /> is a parent of <paramref name="parentId" />. This
    ///   prevents error messages in SuperMemo when adding new elements.
    /// </summary>
    /// <param name="parentId">The parent of the new element</param>
    /// <returns></returns>
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

    /// <summary>
    ///   Calls the native function of SuperMemo and creates an element to <paramref name="elementSpecs" />'s specifications.
    /// </summary>
    /// <param name="type">The element's type (topic, item, etc..)</param>
    /// <param name="elementSpecs">The element's specifications</param>
    /// <returns>The created element id, or -1 if creation failed.</returns>
    private static Task<int> AddElementAsync(ElementType type, string elementSpecs)
    {
      return SysTask.FromResult(Core.SM.UI.ElementWdw.AppendAndAddElementFromText(
                                  type,
                                  elementSpecs));

#if false
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
#endif
    }

    private async Task<(ElemCreationResultCodes resCode, int elemId)> AddElementAsync(ElementBuilder builder,
                                                                                      int            parentId)
    {
      try
      {
        var successFlag = ElemCreationResultCodes.Success;

        //
        // Has a concept been specified for the new element ?

        if (builder.ConceptId.HasValue)
          if (Core.SM.UI.ElementWdw.SetCurrentConcept(builder.ConceptId.Value) == false)
            successFlag |= ElemCreationResultCodes.WarningConceptNotSet;

        //
        // Make sure concept & root are valid

        if (AdjustRoot(parentId))
          successFlag |= ElemCreationResultCodes.WarningConceptNotSet;

        //
        // Set hook *after* adjusting concept

        Core.SM.UI.ElementWdw.CurrentHookId = parentId;

        //
        // Create

        var elemId = await AddElementAsync(builder.Type, builder.ToElementString()).ConfigureAwait(false);

        if (elemId <= 0)
          return (ElemCreationResultCodes.ErrorUnknown, elemId);

        if (builder.TemplateId.HasValue)
        {
          var template = Core.SM.Registry.Template[builder.TemplateId.Value];

          if (template?.Empty == false)
            Core.SM.UI.ElementWdw.ApplyTemplate(template.Id, builder.TemplateApplyMode);
        }

        return (successFlag, elemId);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception caught while adding new element");
        return (ElemCreationResultCodes.ErrorUnknown, -1);
      }
    }

#if false
    // TODO: Implement after finishing the new hook engine
    private Task<bool> AddBulkInternalAsync(
      IEnumerable<ElementBuilder> builder,
      ElemCreationFlags options,
      int                         parentId)
    {
      var successFlag = ElemCreationResultCodes.Success;

      try
      {
        //
        // Create or use auto subfolder, if requested

        parentId = FindDestinationBranch(this[parentId], options);

        if (parentId <= 0 || this[parentId] == null)
          return ElemCreationResultCodes.ErrorTooManyChildren;

        //
        // Has a concept been specified for the new element ?

        if (builder.ConceptId != null)
          if (Core.SM.UI.ElementWdw.SetCurrentConcept(builder.ConceptId) == false)
            successFlag |= ElemCreationResultCodes.WarningConceptNotSet;

        //
        // Make sure concept & root are valid

        if (AdjustRoot(parentId))
          successFlag |= ElemCreationResultCodes.WarningConceptNotSet;

        //
        // Set hook *after* adjusting concept

        Core.SM.UI.ElementWdw.CurrentHookId = parentId;

        //
        // Create

        return AddElement(builder, out elemId)
          ? successFlag
          : ElemCreationResultCodes.ErrorUnknown;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception caught while adding new element");
        return ElemCreationResultCodes.ErrorUnknown;
      }
    }
#endif

    private async SysTask AddInternalAsync(
      List<ElemCreationResult> elements,
      IDestinationBranchFinder branchFinder)
    {
      ElemCreationResultCodes errorCode      = ElemCreationResultCodes.Success;
      int                     parentId       = -1;
      int                     slotsAvailable = 0;

      for (int i = 0; i < elements.Count; i++)
      {
        var elem = elements[i];

        if (slotsAvailable <= 0 && errorCode == ElemCreationResultCodes.Success)
          (parentId, errorCode, slotsAvailable) = await branchFinder.GetOrCreateFolderAsync().ConfigureAwait(false);

        if (errorCode != ElemCreationResultCodes.Success)
        {
          elem.Result    = errorCode;
          elem.ElementId = -1;
        }

        else
        {
          using var c = new ConceptSnapshot();

          var (resCode, elemId) = await AddElementAsync(elem.Builder, parentId).ConfigureAwait(false);

          elem.Result    = resCode;
          elem.ElementId = elemId;
        }
      }
    }

    private async Task<List<ElemCreationResult>> AddInternalAsync(
      ElemCreationFlags options,
      ElementBuilder[]  builders)
    {
      if (builders == null || builders.Length == 0)
        return null;

      var inSMUpdateLockMode  = false;
      var inSMAUpdateLockMode = false;
      var singleMode          = builders.Length == 1;

      var results = new List<ElemCreationResult>(
        builders.Select(
          b => new ElemCreationResult(ElemCreationResultCodes.ErrorUnknown, b))
      );

      try
      {
        int restoreElementId = Core.SM.UI.ElementWdw.CurrentElementId;
        int restoreHookId    = Core.SM.UI.ElementWdw.CurrentHookId;

        //
        // Enter critical section

        _addMutex.WaitOne();

        //
        // Suspend element changed monitoring

        inSMAUpdateLockMode = Core.SM.UI.ElementWdw.EnterSMAUpdateLock();

        //
        // Focus

        // toDispose.Add(new FocusSnapshot(true)); // TODO: Only if inserting 1 element

        //
        // Regroup by parent id to optimize remote calls

        var resultsByParent = results.GroupBy(k => k.Builder.ParentId);

        //
        // Freeze element window if we want to insert the element without displaying it immediately

        if (singleMode == false || builders[0].ShouldDisplay == false)
          inSMUpdateLockMode = Core.SM.UI.ElementWdw.EnterSMUpdateLock(); // TODO: Pass in EnterUpdateLock

        foreach (var rpGroup in resultsByParent)
        {
          using var h = new HookSnapshot();

          var parent = rpGroup.Key.HasValue ? this[rpGroup.Key.Value] : this[restoreHookId];

          var branchFinder = options.HasFlag(ElemCreationFlags.CreateSubfolders)
            ? (IDestinationBranchFinder)new SubfolderBranchFinder(parent, options)
            : (IDestinationBranchFinder)new ConstantBranchFinder(parent);

          await AddInternalAsync(rpGroup.ToList(), branchFinder).ConfigureAwait(false);
        }

        //
        // Display original element, and unfreeze window -- or simply resume element changed monitoring

        if (inSMUpdateLockMode)
        {
          inSMUpdateLockMode = Core.SM.UI.ElementWdw.QuitSMUpdateLock() == false;

          Core.SM.UI.ElementWdw.GoToElement(restoreElementId);
        }

        inSMAUpdateLockMode = Core.SM.UI.ElementWdw.QuitSMAUpdateLock(inSMUpdateLockMode);

        return results;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "An exception was thrown while creating a new element in SM.");
        return results;
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
    // Internal registry works

    protected ElementBase GetInternal(int id) => Elements.SafeGet(id);

    protected void OnElementCreatedInternal(ElementBase el)
    {
      try
      {
        OnElementCreated?.Invoke(new SMElementEventArgs(Core.SM, el));
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
          OnElementDeleted?.Invoke(new SMElementEventArgs(Core.SM, el));

        else
          OnElementModified?.Invoke(new SMElementChangedEventArgs(Core.SM, el, flags));
      }
      catch (Exception ex)
      {
        var eventType = deleted ? "Deleted" : "Modified";

        LogTo.Error(ex, "Error while signaling Element {EventType} event", eventType);
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

    internal async Task<int> WaitForNextElementAsync(int timeOutMs = 3000)
    {
      using (var cts = new CancellationTokenSource(timeOutMs))
        return await WaitForNextElementAsync(cts.Token).ConfigureAwait(false);
    }

    internal async Task<int> WaitForNextElementAsync(CancellationToken ct)
    {
      try
      {
        _waitForElementId = int.MinValue;
        _waitForElementAnyEvent.Reset();

        await _waitForElementAnyEvent.WaitAsync(ct).ConfigureAwait(false);

        return ct.IsCancellationRequested ? -1 : _waitForElementResultId;
      }
      finally
      {
        _waitForElementId = -1;
      }
    }

    internal async Task<int> WaitForNextCreatedElementAsync(int timeOutMs = 3000)
    {
      using (var cts = new CancellationTokenSource(timeOutMs))
        return await WaitForNextCreatedElementAsync(cts.Token).ConfigureAwait(false);
    }

    internal async Task<int> WaitForNextCreatedElementAsync(CancellationToken ct)
    {
      try
      {
        _waitForElementId = int.MaxValue;
        _waitForElementCreatedEvent.Reset();

        await _waitForElementCreatedEvent.WaitAsync(ct).ConfigureAwait(false);

        return ct.IsCancellationRequested ? -1 : _waitForElementResultId;
      }
      finally
      {
        _waitForElementId = -1;
      }
    }

    internal async Task<int> WaitForNextUpdatedElementAsync(int timeOutMs = 3000)
    {
      using (var cts = new CancellationTokenSource(timeOutMs))
        return await WaitForNextUpdatedElementAsync(cts.Token).ConfigureAwait(false);
    }

    internal async Task<int> WaitForNextUpdatedElementAsync(CancellationToken ct)
    {
      try
      {
        _waitForElementId = int.MinValue;
        _waitForElementUpdatedEvent.Reset();

        await _waitForElementUpdatedEvent.WaitAsync(ct).ConfigureAwait(false);

        return ct.IsCancellationRequested ? -1 : _waitForElementResultId;
      }
      finally
      {
        _waitForElementId = -1;
      }
    }

    // TODO: Make this thread-safe
    internal async Task<bool> WaitForElementAsync(int elemId, string title, int timeOutMs = 3000)
    {
      try
      {
        _waitForElementIdEvent.Reset();
        _waitForElementId = elemId;

        var elem = this[elemId];

        if (elem != null && elem.Title == title)
          return true;

        using var cts = new CancellationTokenSource(timeOutMs);

        try
        {
          await _waitForElementIdEvent.WaitAsync(cts.Token).ConfigureAwait(false);

          return true;
        }
        catch
        {
          return false;
        }
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

    public event Action<SMElementEventArgs>        OnElementCreated;
    public event Action<SMElementEventArgs>        OnElementDeleted;
    public event Action<SMElementChangedEventArgs> OnElementModified;

    #endregion




    private interface IDestinationBranchFinder
    {
      /// <summary>Returns the next parent available for insertion.</summary>
      /// <returns>
      ///   The next parent id. If an error occurs, the parent id is set to -1 and errorCode describes the error type.
      /// </returns>
      Task<(int parentId, ElemCreationResultCodes errorCode, int slotsAvailable)> GetOrCreateFolderAsync();

      /// <summary>Advances the iterator across folders for a count of <paramref name="count" />.</summary>
      /// <param name="count">The number of slots "consumed" (how many children were inserted)</param>
      void Consume(int count);
    }

    private class ConstantBranchFinder : IDestinationBranchFinder
    {
      #region Properties & Fields - Non-Public

      private readonly IElement _parent;
      private          int      _slotsLeft;

      #endregion




      #region Constructors

      public ConstantBranchFinder(IElement parent)
      {
        _parent    = parent;
        _slotsLeft = _parent.GetAvailableSlots();
      }

      #endregion




      #region Methods Impl

      /// <inheritdoc />
      public Task<(int parentId, ElemCreationResultCodes errorCode, int slotsAvailable)> GetOrCreateFolderAsync()
      {
        var errorCode = _slotsLeft > 0
          ? ElemCreationResultCodes.Success
          : ElemCreationResultCodes.ErrorTooManyChildren;
        var res = (_parent.Id, errorCode, _slotsLeft);

        return SysTask.FromResult(res);
      }

      /// <inheritdoc />
      public void Consume(int count)
      {
        _slotsLeft -= count;
      }

      #endregion
    }

    /// <summary>Computes and creates auto subfolder structures</summary>
    private class SubfolderBranchFinder : IDestinationBranchFinder
    {
      #region Properties & Fields - Non-Public

      private readonly short               _maxChildren;
      private readonly IElement            _parent;
      private readonly List<SubfolderData> _subFolders;
      private          int                 _it;

      private Regex _titleRegex;

      private Regex TitleRegex => _titleRegex ??= new Regex($"\\[([0-9]+)\\] {Regex.Escape(_parent.Title)}");

      #endregion




      #region Constructors

      public SubfolderBranchFinder(
        IElement          parent,
        ElemCreationFlags options)
      {
        _parent = parent;

        _maxChildren = Core.SM.UI.ElementWdw.LimitChildrenCount;
        _subFolders = parent.Children
                            .Choose(TryMatchSubfolder)
                            .ToList();

        _it = options.HasFlag(ElemCreationFlags.ReuseSubFolders)
          ? 1
          : Math.Max(1, _subFolders.Count);

        //while (newElemCount > 0)
        //{
        //  SubfolderData data;

        //  if (i < _subFolders.Count)
        //  {
        //    var (child, sfNo) = _subFolders[i];

        //    data = new SubfolderData(child.Id, sfNo, _maxChildren - child.ChildrenCount);
        //  }

        //  else
        //  {
        //    data = new SubfolderData(null, ++folderCount, _maxChildren);
        //  }

        //  newElemCount -= data.SlotsLeft;
        //  _data.Add(data);
        //}
      }

      #endregion




      #region Methods Impl

      /// <inheritdoc />
      public async Task<(int parentId, ElemCreationResultCodes errorCode, int slotsAvailable)> GetOrCreateFolderAsync()
      {
        //
        // Try & find the next available subfolder (if any)

        while (_it <= _subFolders.Count)
        {
          var curSubFolder = _subFolders[_it - 1];

          if (curSubFolder.SlotsLeft > 0)
            return (curSubFolder.ElementId, ElemCreationResultCodes.Success, curSubFolder.SlotsLeft);

          _it++;
        }

        //
        // Existing subfolders have reached capacity, or we chose not to re-use them

        // Parent has reached capacity
        if (_it > _maxChildren)
          return (-1, ElemCreationResultCodes.ErrorDestinationBranchNotCreated | ElemCreationResultCodes.ErrorTooManyChildren, 0);

        var subFolderId = await CreateFolderAsync().ConfigureAwait(false);

        // Failed to create subfolder
        if (subFolderId < 0)
          return (-1, ElemCreationResultCodes.ErrorDestinationBranchNotCreated | ElemCreationResultCodes.ErrorUnknown, 0);

        // Successfully created the new subfolder, add it to the list & return with info
        _subFolders.Add(new SubfolderData(subFolderId, _maxChildren));

        return (subFolderId, ElemCreationResultCodes.Success, _maxChildren);
      }

      /// <inheritdoc />
      public void Consume(int count)
      {
        if (_it > _subFolders.Count)
          throw new InvalidOperationException("Consuming sub folder out of range");

        var curSubFolder = _subFolders[_it - 1];

        if (count > curSubFolder.SlotsLeft)
          throw new ArgumentException(
            $"Unable to count {count} slots in destination branch: only {curSubFolder.SlotsLeft} slots are available in current branch",
            nameof(count));

        curSubFolder.SlotsLeft -= count;
      }

      #endregion




      #region Methods

      /// <summary>
      ///   Tries to create the next subfolder. Expects the parent to have slots left, and _it to reflect the new subfolder
      ///   number.
      /// </summary>
      /// <returns>The subfolder element id or -1 if creation failed.</returns>
      private async Task<int> CreateFolderAsync()
      {
        var title = $"[{_it}] {_parent.Title}";

        var (_, elemId) = await Core.SM.Registry.Element.AddElementAsync(
          new ElementBuilder(ElementType.Topic)
            .WithTitle(title)
            .WithStatus(ElementStatus.Dismissed)
            .WithPriority(100.0)
            .WithConcept(_parent.Concept)
            .DoNotDisplay(), // TODO: Remove? That shouldn't matter, lock mode is already either set or not
          _parent.Id
        ).ConfigureAwait(false);

        return elemId;
      }

      private (bool, SubfolderData) TryMatchSubfolder(IElement subfolder)
      {
        if (subfolder.Deleted || string.IsNullOrWhiteSpace(subfolder.Title))
          return (false, default);

        var m = TitleRegex.Match(subfolder.Title);

        if (m.Success == false)
          return (false, default);

        var subfolderNo = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);

        return (true, new SubfolderData(subfolder.Id, GetAvailableSlots(subfolder)));
      }

      private int GetAvailableSlots(IElement elem)
      {
        return _maxChildren - elem.ChildrenCount;
      }

      #endregion
    }


    /// <summary>Contains data about a subfolder, see <see cref="SubfolderBranchFinder" />.</summary>
    private class SubfolderData
    {
      #region Constructors

      public SubfolderData(int elementId, int slotsLeft)
      {
        ElementId   = elementId;
        SlotsLeft   = slotsLeft;
      }

      #endregion




      #region Properties & Fields - Public

      public int ElementId   { get; }
      public int SlotsLeft   { get; set; }

      #endregion
    }
  }
}
