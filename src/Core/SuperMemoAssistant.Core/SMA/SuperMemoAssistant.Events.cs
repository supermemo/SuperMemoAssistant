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
// Created On:   2020/01/11 19:03
// Modified On:  2020/01/11 19:03
// Modified By:  Alexis

#endregion




using System;
using System.Threading.Tasks;
using Anotar.Serilog;
using AsyncEvent;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SMA.UI;

namespace SuperMemoAssistant.SMA
{
  public partial class SMA
  {
    #region Methods

    public async Task OnSMStarting()
    {
      try
      {
        if (OnSMStartingEvent != null)
          await OnSMStartingEvent.InvokeAsync(this, new SMEventArgs(_sm)).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception while notifying subscribers of OnSMStarting");
        throw;
      }
    }

    public async Task OnSMStarted()
    {
      try
      {
        await SaveConfig(false).ConfigureAwait(false);

        SMAUI.Initialize();

        if (OnSMStartedEvent != null)
          await OnSMStartedEvent.InvokeAsync(
            this,
            new SMProcessArgs(_sm, SMProcess.Native)).ConfigureAwait(true);
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
        OnSMStoppedEvent?.Invoke(this, null);
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

    public event AsyncEventHandler<SMProcessArgs> OnSMStartedEvent;
    public event AsyncEventHandler<SMEventArgs>   OnSMStartingEvent;
    public event EventHandler<SMProcessArgs> OnSMStoppedEvent;

    #endregion
  }
}
