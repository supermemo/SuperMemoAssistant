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
// Created On:   2018/05/08 22:27
// Modified On:  2018/05/30 23:35
// Modified By:  Alexis

#endregion




using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.EventHandlers;
using FlaUI.Core.Exceptions;
using FlaUI.Core.Identifiers;
using FlaUI.UIA3;
using FlaUI.UIA3.EventHandlers;
using Nito.AsyncEx;
using Process.NET;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Sys;
using SuperMemoAssistant.Services;
using EventList =
  System.Collections.Generic.List<(FlaUI.Core.AutomationElements.AutomationElement parent,
    FlaUI.Core.EventHandlers.AutomationEventHandlerBase handler)>;
using WaitersDictionary =
  System.Collections.Concurrent.ConcurrentDictionary<System.Func<FlaUI.Core.AutomationElements.AutomationElement, bool>,
    (FlaUI.Core.AutomationElements.AutomationElement res, Nito.AsyncEx.AsyncAutoResetEvent ev)>;

namespace SuperMemoAssistant.Sys.UIAutomation
{
  [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
  public abstract class UIAutomationBase : SMMarshalByRefObject, IDisposable
  {
    #region Properties & Fields

    //
    // UI Automation - Event registration

    protected readonly EventList      RegisteredEvents = new EventList();
    protected          UIA3Automation UIAuto => Svc.UIAutomation;

    private WaitersDictionary                WindowOpenedWaiters            { get; } = new WaitersDictionary();
    private WaitersDictionary                WindowClosedWaiters            { get; } = new WaitersDictionary();
    private WaitersDictionary                ElementAddedWaiters            { get; } = new WaitersDictionary();
    private WaitersDictionary                ElementRemovedWaiters          { get; } = new WaitersDictionary();
    private AutomationElement                AwaiterParent                  { get; set; }
    private StructureChangedEventHandlerBase AwaiterStructureChangedHandler { get; set; }
    private AutomationEventHandlerBase       AwaiterWindowOpenedHandler     { get; set; }
    private AutomationEventHandlerBase       AwaiterWindowClosedHandler     { get; set; }

    #endregion




    #region Constructors

    //
    // Lifecycle

    protected UIAutomationBase() { }

    /// <inheritdoc />
    public virtual void Dispose()
    {
      UnregisterUIAutomationEvents();
    }

    #endregion




    #region Methods

    protected void RegisterAwaitersEvents(AutomationElement root)
    {
      AwaiterParent = root;

      AwaiterStructureChangedHandler = root.RegisterStructureChangedEvent(
        TreeScope.Children,
        OnStructureChanged
      );

      AwaiterWindowOpenedHandler = RegisterAutomationEventManual(
        root,
        UIAuto.EventLibrary.Window.WindowOpenedEvent,
        TreeScope.Children | TreeScope.Element,
        IsSMProcess,
        Awaiter_OnWindowOpened
      );

      AwaiterWindowClosedHandler = RegisterAutomationEventManual(
        root,
        UIAuto.EventLibrary.Window.WindowClosedEvent,
        TreeScope.Children | TreeScope.Element,
        IsSMProcess,
        Awaiter_OnWindowClosed
      );
    }

    protected void UnregisterAwaitersEvents()
    {
      if (AwaiterStructureChangedHandler != null)
        AwaiterParent.FrameworkAutomationElement.UnregisterStructureChangedEventHandler(AwaiterStructureChangedHandler);

      if (AwaiterWindowOpenedHandler != null)
        UnregisterAutomationEvent(AwaiterParent, AwaiterWindowOpenedHandler);

      if (AwaiterWindowClosedHandler != null)
        UnregisterAutomationEvent(AwaiterParent, AwaiterWindowClosedHandler);

      AwaiterParent                  = null;
      AwaiterStructureChangedHandler = null;
      AwaiterWindowOpenedHandler     = null;
      AwaiterWindowClosedHandler     = null;
    }


    // TODO:
    // - Freeze UI from user's interaction (?)
    // - Internal UI lock to avoid multiple interactions (?)


    //
    // UI Automation - Filters

    public Task<(bool success, AutomationElement elem)> WaitWdwOpenedAsync(
      Func<AutomationElement, bool> filter,
      int                           timeOut = Timeout.Infinite)
    {
      return Awaiter_WaitAsync(WindowOpenedWaiters, filter, timeOut);
    }

    public Task<(bool success, AutomationElement elem)> WaitWdwClosedAsync(
      Func<AutomationElement, bool> filter,
      int                           timeOut = Timeout.Infinite)
    {
      return Awaiter_WaitAsync(WindowClosedWaiters, filter, timeOut);
    }

    public Task<(bool success, AutomationElement elem)> WaitElementAddedAsync(
      Func<AutomationElement, bool> filter,
      int                           timeOut = Timeout.Infinite)
    {
      return Awaiter_WaitAsync(ElementAddedWaiters, filter, timeOut);
    }

    public Task<(bool success, AutomationElement elem)> WaitElementRemovedAsync(
      Func<AutomationElement, bool> filter,
      int                           timeOut = Timeout.Infinite)
    {
      return Awaiter_WaitAsync(ElementRemovedWaiters, filter, timeOut);
    }


    //
    // UI Events

    private void Awaiter_OnWindowOpened(AutomationElement ae, EventId _)
    {
      Awaiter_DoFilter(ae, WindowOpenedWaiters);
    }

    private void Awaiter_OnWindowClosed(AutomationElement ae, EventId _)
    {
      Awaiter_DoFilter(ae, WindowClosedWaiters);
    }

    private void OnStructureChanged(AutomationElement ae, StructureChangeType changeType, int[] values)
    {
      switch (changeType)
      {
        case StructureChangeType.ChildAdded:
        case StructureChangeType.ChildrenBulkAdded:
          Awaiter_DoFilter(ae, ElementAddedWaiters);
          break;

        case StructureChangeType.ChildRemoved:
        case StructureChangeType.ChildrenBulkRemoved:
          Awaiter_DoFilter(ae, ElementRemovedWaiters);
          break;
      }
    }


    //
    // Helpers

    protected void Awaiter_DoFilter(
      AutomationElement ae,
      WaitersDictionary waiters)
    {
      foreach (var filterFunc in waiters.Keys.ToList())
        if (filterFunc(ae))
        {
          var awaiterData = waiters.SafeGet(filterFunc);

          awaiterData.res     = ae;
          waiters[filterFunc] = awaiterData;

          awaiterData.ev.Set();
        }
    }

    protected async Task<(bool success, AutomationElement elem)> Awaiter_WaitAsync(
      WaitersDictionary             waiters,
      Func<AutomationElement, bool> filter,
      int                           timeOut)
    {
      AsyncAutoResetEvent ev = new AsyncAutoResetEvent(false);

      ValueTuple<AutomationElement, AsyncAutoResetEvent> awaiterData = (null, ev);
      waiters[filter] = awaiterData;

      var cts  = new CancellationTokenSource(timeOut);
      var task = ev.WaitAsync(cts.Token);

      await task;

      cts.Dispose();

      waiters.TryRemove(filter, out awaiterData);

      if (task.IsCanceled)
        return (false, null);

      var ret = awaiterData.Item1;

      return (true, ret);
    }


    //
    // UI Automation - Filters

    protected bool IsSMProcess(AutomationElement automationElement, EventId _)
    {
      try
      {
        return automationElement.Properties.ProcessId == SMProcess.Native.Id;
      }
      catch (System.Runtime.InteropServices.COMException)
      {
        return false;
      }
      catch (PropertyNotSupportedException)
      {
        return false;
      }
    }

    protected Func<AutomationElement, EventId, bool> IsSMWindow(string windowClass)
    {
      bool MatchClassName(AutomationElement ae, EventId _)
      {
        try
        {
          return Equals(ae.ClassName, windowClass);
        }
        catch (System.Runtime.InteropServices.COMException)
        {
          return false;
        }
        catch (PropertyNotSupportedException)
        {
          return false;
        }
      }

      return MatchClassName;
    }


    //
    // UI Automation - Helpers

#if false
    protected Action<AutomationElement, EventId> WrapAutomationEvent(
      EventHandler<SMUIAutomationArgs> automationEvent)
    {
      return (ae, _) => automationEvent?.Invoke(this, new SMUIAutomationArgs(SMA.Instance, ae));
    }
  #endif

    /// <summary>Request an Auto-Release notifification about <paramref name="eventId" /></summary>
    /// <param name="element">Target element</param>
    /// <param name="eventId">Event to be notified about</param>
    /// <param name="treeScope">Scope to monitor</param>
    /// <param name="withFilter">(Optional) Criteria to meet for calling
    ///   <param name="actions" />
    /// </param>
    /// <param name="actions">Callbacks</param>
    /// <returns></returns>
    public AutomationEventHandlerBase RegisterAutomationEvent(
      AutomationElement                           element,
      EventId                                     eventId,
      TreeScope                                   treeScope,
      Func<AutomationElement, EventId, bool>      withFilter = null,
      params Action<AutomationElement, EventId>[] actions)
    {
      return RegisterAutomationEventInternal(element, eventId, treeScope, true, withFilter, actions);
    }

    /// <summary>Request a Manual-Release notifification about <paramref name="eventId" /></summary>
    /// <param name="element">Target element</param>
    /// <param name="eventId">Event to be notified about</param>
    /// <param name="treeScope">Scope to monitor</param>
    /// <param name="withFilter">(Optional) Criteria to meet for calling
    ///   <param name="actions" />
    /// </param>
    /// <param name="actions">Callbacks</param>
    /// <returns></returns>
    public AutomationEventHandlerBase RegisterAutomationEventManual(
      AutomationElement                           element,
      EventId                                     eventId,
      TreeScope                                   treeScope,
      Func<AutomationElement, EventId, bool>      withFilter = null,
      params Action<AutomationElement, EventId>[] actions)
    {
      return RegisterAutomationEventInternal(element, eventId, treeScope, false, withFilter, actions);
    }

    /// <summary>Request notififications about <paramref name="eventId" /></summary>
    /// <param name="element">Target element</param>
    /// <param name="eventId">Event to be notified about</param>
    /// <param name="treeScope">Scope to monitor</param>
    /// <param name="autoRelease">Whether to automatically unregister event when object is Disposed</param>
    /// <param name="withFilter">(Optional) Criteria to meet for calling
    ///   <param name="actions" />
    /// </param>
    /// <param name="actions">Callbacks</param>
    /// <returns></returns>
    protected AutomationEventHandlerBase RegisterAutomationEventInternal(
      AutomationElement                           element,
      EventId                                     eventId,
      TreeScope                                   treeScope,
      bool                                        autoRelease,
      Func<AutomationElement, EventId, bool>      withFilter = null,
      params Action<AutomationElement, EventId>[] actions)
    {
      void ActionWrapper(AutomationElement ae, EventId e)
      {
        if (withFilter == null || withFilter(ae, e))
          foreach (var action in actions)
            action(ae, e);
      }

      return RegisterAutomationEventInternal(element, eventId, treeScope, autoRelease,
                                             (Action<AutomationElement, EventId>)ActionWrapper);
    }

    /// <summary>Request notififications about <paramref name="eventId" /></summary>
    /// <param name="element">Target element</param>
    /// <param name="eventId">Event to be notified about</param>
    /// <param name="treeScope">Scope to monitor</param>
    /// <param name="autoRelease">Whether to automatically unregister event when object is Disposed</param>
    /// <param name="action">Callback</param>
    /// <returns></returns>
    protected AutomationEventHandlerBase RegisterAutomationEventInternal(
      AutomationElement                  element,
      EventId                            eventId,
      TreeScope                          treeScope,
      bool                               autoRelease,
      Action<AutomationElement, EventId> action)
    {
      var automationElement = element ?? UIAuto.GetDesktop();
      var desktopEl         = (UIA3FrameworkAutomationElement)automationElement.FrameworkAutomationElement;
      var eventHandler      = new UIA3AutomationEventHandler(desktopEl, eventId, action);

      UIAuto.NativeAutomation.AddAutomationEventHandler(
        eventId.Id,
        desktopEl.NativeElement,
        (global::Interop.UIAutomationClient.TreeScope)treeScope,
        null,
        eventHandler
      );

      if (autoRelease)
      {
        RegisteredEvents.Add((automationElement, eventHandler));
        return null;
      }

      return null;
    }

    /// <summary>Unregister callback with NativeAutomation</summary>
    /// <param name="parent"></param>
    /// <param name="eventHandler">
    ///   Event handler to unregister given event handler, usually obtained
    ///   with RegisterAutomationEvent
    /// </param>
    public void UnregisterAutomationEvent(AutomationElement parent, ElementEventHandlerBase eventHandler)
    {
      var frameworkEventHandler = (UIA3AutomationEventHandler)eventHandler;

      UIAuto.NativeAutomation.RemoveAutomationEventHandler(
        frameworkEventHandler.Event.Id,
        ((UIA3FrameworkAutomationElement)parent.FrameworkAutomationElement).NativeElement,
        frameworkEventHandler
      );
    }

    /// <summary>
    ///   Call
    ///   <see
    ///     cref="UIAutomationBase.UnregisterAutomationEvent(FlaUI.Core.AutomationElements.AutomationElement,FlaUI.Core.EventHandlers.ElementEventHandlerBase)" />
    ///   for each register auto releasing event.
    /// </summary>
    private void UnregisterUIAutomationEvents()
    {
      foreach (var handlerData in RegisteredEvents)
        UnregisterAutomationEvent(handlerData.parent, handlerData.handler);

      RegisteredEvents.Clear();
    }

    #endregion




    #region Methods Abs

    public abstract IProcess SMProcess { get; set; }

    #endregion
  }
}
