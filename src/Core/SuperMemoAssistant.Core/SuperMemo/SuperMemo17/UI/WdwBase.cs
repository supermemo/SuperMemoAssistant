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
// Created On:   2018/05/24 13:30
// Modified On:  2018/12/27 21:49
// Modified By:  Alexis

#endregion




using System;
using Process.NET;
using Process.NET.Windows;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.UI;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.UI
{
  public abstract class WdwBase : /*UIAutomationBase,*/MarshalByRefObject, IDisposable, IWdw
  {
    #region Properties & Fields - Non-Public

    //internal AutomationElement Window { get; set; }
    private IWindow _window = null;
    public  IWindow Window => _window ?? (_window = GetWindow());

    #endregion




    private IWindow GetWindow()
    {
      return new RemoteWindow(SMProcess,
                              WindowHandle);
    }




    #region Constructors

    protected WdwBase()
    {
      SMA.Instance.OnSMStartedEvent += OnSMStarted;
      SMA.Instance.OnSMStoppedEvent += OnSMStopped;
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
      //CleanupUI();

      //base.Dispose();
    }

    #endregion




    #region Properties Impl - Public

    public IProcess SMProcess { get; set; }

    //public AutomationElementRef AutomationElement => new AutomationElementRef(Window);

    #endregion




    #region Methods

    private void OnSMStarted(object        sender,
                             SMProcessArgs e)
    {
      SMProcess = e.Process;
      //SetupUI();
    }

    private void OnSMStopped(object        sender,
                             SMProcessArgs e)
    {
      //CleanupUI();

      //if (Window != null)
      //  OnWindowClosed(Window,
      //                 UIAuto.EventLibrary.Window.WindowClosedEvent);
    }

    //
    // Helpers

#if false
// ReSharper disable once InconsistentNaming
    protected Task<bool> ExecuteMenuAction<TMenu, ITMenu>(TMenu          menu,
                                                          Action<ITMenu> action)
      where TMenu : MenuRoot, ITMenu
      where ITMenu : IMenu
    {
      Window.Focus();

      action(menu);

      return menu.Execute();
    }


    //
    // UI Automation Core

    protected virtual void SetupUI()
    {
      RegisterUIAutomationEvents();
    }

    protected virtual void CleanupUI() { }

    private void RegisterUIAutomationEvents()
    {
      // WindowOpenedEvent
      RegisterAutomationEvent(
        null,
        UIAuto.EventLibrary.Window.WindowOpenedEvent,
        TreeScope.Children,
        IsSMWindow(WindowClass),
        OnWindowOpened);

      // WindowClosedEvent
      RegisterAutomationEvent(
        null,
        UIAuto.EventLibrary.Window.WindowClosedEvent,
        TreeScope.Children,
        IsSMWindow(WindowClass),
        OnWindowClosed);

      // TODO: Find how to detect minimized/restored
    }


    //
    // UI Automation Events

    /// <summary>Notification for window open event.</summary>
    /// <param name="elem"></param>
    /// <param name="eventId"></param>
    protected virtual void OnWindowOpened(AutomationElement elem,
                                          EventId           eventId)
    {
      Window = elem;

      RegisterAwaitersEvents(Window);

      try
      {
        OnAvailable?.Invoke(new SMUIAvailabilityArgs(SMA.Instance,
                                                     AvailabilityType.Opened));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while signaling Window Available event");
      }
    }

    /// <summary>Notification for window close event.</summary>
    /// <param name="elem"></param>
    /// <param name="eventId"></param>
    protected virtual void OnWindowClosed(AutomationElement elem,
                                          EventId           eventId)
    {
      Window = null;

      UnregisterAwaitersEvents();

      try
      {
        OnUnavailable?.Invoke(new SMUIAvailabilityArgs(SMA.Instance,
                                                             AvailabilityType.Closed));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while signaling Window Unavailable event");
      }
    }
#endif

    #endregion




    #region Methods Abs

    //
    // Inheritance

    public abstract string WindowClass { get; }

    protected abstract IntPtr WindowHandle { get; }

    #endregion




    #region Events

    //
    // IWdw Events

    /// <inheritdoc />
    public event Action<SMUIAvailabilityArgs> OnAvailable;
    /// <inheritdoc />
    public event Action<SMUIAvailabilityArgs> OnUnavailable;

    #endregion
  }
}
