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




namespace SuperMemoAssistant.SMA
{
  using System;
  using System.Threading.Tasks;
  using Anotar.Serilog;
  using AsyncEvent;
  using Extensions;
  using Interop.SuperMemo.Core;

  public partial class SMA
  {
    #region Methods

    public async Task OnCollectionSelectedAsync(SMCollection collection)
    {
      try
      {
        _sm.UI.ElementWdw.OnAvailableInternal += OnSuperMemoWindowsAvailable;

        if (OnCollectionSelectedInternalEvent != null)
          await OnCollectionSelectedInternalEvent.InvokeAsync(this, new SMEventArgs(_sm)).ConfigureAwait(false);

        OnCollectionSelectedEvent?.InvokeRemote(
          nameof(OnCollectionSelectedAsync),
          collection,
          h => OnCollectionSelectedEvent -= h
        );
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception while notifying subscribers of OnCollectionSelected");
        throw;
      }
    }

    public async Task OnSMStartingAsync()
    {
      try
      {
        if (OnSMStartingInternalEvent != null)
          await OnSMStartingInternalEvent.InvokeAsync(this, new SMEventArgs(_sm)).ConfigureAwait(false);

        OnSMStartingEvent?.InvokeRemote(
          nameof(OnSMStartingEvent),
          h => OnSMStartingEvent -= h
        );
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception while notifying subscribers of OnSMStarting");
        throw;
      }
    }

    public async Task OnSMStartedAsync()
    {
      try
      {
        await SaveConfigAsync().ConfigureAwait(false);

        if (OnSMStartedInternalEvent != null)
          await OnSMStartedInternalEvent.InvokeAsync(
            this,
            new SMProcessEventArgs(_sm, SMProcess.Native)).ConfigureAwait(false);

        OnSMStartedEvent?.InvokeRemote(
          nameof(OnSMStartingEvent),
          h => OnSMStartingEvent -= h
        );
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception while notifying subscribers of OnSMStarted");
        throw;
      }
    }

    public void OnSMStopped()
    {
      _sm = null;

      try
      {
        OnSMStoppedInternalEvent?.Invoke(this, null);

        OnSMStoppedEvent?.InvokeRemote(
          nameof(OnSMStartingEvent),
          h => OnSMStartingEvent -= h
        );
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception thrown while notifying subscribers of OnSMStoppedEvent. Killing process");
        System.Diagnostics.Process.GetCurrentProcess().Kill();
      }
    }

    private void OnSuperMemoWindowsAvailable()
    {
      ApplySuperMemoWindowStyles();
    }

    #endregion




    #region Events

    /// <inheritdoc />
    public event Action<SMCollection> OnCollectionSelectedEvent;
    public event AsyncEventHandler<SMEventArgs> OnCollectionSelectedInternalEvent;
    /// <inheritdoc />
    public event Action OnSMStartedEvent;
    public event AsyncEventHandler<SMProcessEventArgs> OnSMStartedInternalEvent;
    /// <inheritdoc />
    public event Action OnSMStartingEvent;
    public event AsyncEventHandler<SMEventArgs> OnSMStartingInternalEvent;
    /// <inheritdoc />
    public event Action OnSMStoppedEvent;
    public event EventHandler<SMProcessEventArgs> OnSMStoppedInternalEvent;

    #endregion
  }
}
