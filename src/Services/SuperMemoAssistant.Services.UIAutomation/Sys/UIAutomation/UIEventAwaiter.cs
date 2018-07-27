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
// Created On:   2018/05/25 00:42
// Modified On:  2018/05/31 10:50
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.EventHandlers;
using Nito.AsyncEx;
using Process.NET;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Sys.UIAutomation
{
  public class UIEventAwaiter : UIAutomationBase
  {
    #region Constants & Statics

    public static UIEventAwaiter Instance { get; } = new UIEventAwaiter();

    #endregion




    #region Properties & Fields - Non-Public

    private ConcurrentDictionary<Func<AutomationElement, bool>, (AutomationElement res, AsyncAutoResetEvent ev)>
      WdwActiveWaiters { get; } =
      new ConcurrentDictionary<Func<AutomationElement, bool>, (AutomationElement, AsyncAutoResetEvent)>();
    protected FocusChangedEventHandlerBase FocusChangedHandler { get; set; }

    #endregion




    #region Constructors

    protected UIEventAwaiter()
    {
      FocusChangedHandler = UIAuto.RegisterFocusChangedEvent(Awaiter_OnFocusChanged);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
      UIAuto.UnregisterFocusChangedEvent(FocusChangedHandler);

      base.Dispose();
    }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override IProcess SMProcess { get => Svc.SMA.SMProcess; set => throw new InvalidOperationException(); }

    #endregion




    #region Methods

    //
    // Core

    public Task<(bool success, AutomationElement elem)> WaitFocusAsync(
      Func<AutomationElement, bool> filter,
      int                           timeOut = Timeout.Infinite)
    {
      return Awaiter_WaitAsync(WdwActiveWaiters, filter, timeOut);
    }


    //
    // UI Events

    private void Awaiter_OnFocusChanged(AutomationElement ae)
    {
      Awaiter_DoFilter(ae, WdwActiveWaiters);
    }

    #endregion
  }
}
