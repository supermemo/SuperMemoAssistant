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





namespace SuperMemoAssistant.SuperMemo.SuperMemo17.UI
{
  using System;
  using Anotar.Serilog;
  using Common.UI;
  using Process.NET.Memory;
  using SuperMemoAssistant.Interop;
  using SuperMemoAssistant.Interop.SuperMemo.Core;
  using SuperMemoAssistant.Interop.SuperMemo.UI.Content;
  using SuperMemoAssistant.SMA;
  using System.Threading.Tasks;
  using System.ComponentModel;

  public sealed class ContentWdw : WdwBase, IDisposable, IContentWdw
  {
    #region Properties & Fields - Non-Public
    private IPointer TContentWdwPtr { get; set; }
    #endregion




    #region Constructors

    public ContentWdw() 
    {
      Core.SMA.OnSMStartedEvent += OnSMStartedEventAsync;
      Core.SMA.OnSMStoppedEvent += OnSMStoppedEvent;
    }

    /// <inheritdoc />
    public void Dispose()
    {
      LogTo.Debug("Cleaning up {Name}", GetType().Name);
      
      TContentWdwPtr?.Dispose();


      TContentWdwPtr = null;

      LogTo.Debug("Cleaning up {Name}... Done", GetType().Name);
    }

    #endregion




    #region Methods Impl
    public bool MoveElementToConcept(int elementId, int conceptId)
    {
      try
      {
        return Core.Natives.Contents.MoveElementToConcept(
          TContentWdwPtr.Read<IntPtr>(),
          elementId,
          conceptId);
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read TContentWdw");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return false;
      }
    }
    #endregion




    #region Methods
    private async Task OnSMStartedEventAsync(object sender,
                                             SMProcessEventArgs e)
    {
      LogTo.Debug("Initializing {Name}", GetType().Name);

      await Task.Run(() =>
      {
        TContentWdwPtr = SMProcess[Core.Natives.Contents.InstancePtr];
      }).ConfigureAwait(false);
    }

    private void OnSMStoppedEvent(object sender,
                                 SMProcessEventArgs e)
    {
      Dispose();
    }

    #endregion




    #region Events
    public override event Action OnAvailable;
    #endregion



    
    #region Properties Impl

    //
    // IWdwBase Impl

    /// <inheritdoc/>
    public override string WindowClass => SMConst.UI.ContentsWindowClassName;

    /// <inheritdoc/>
    protected override IntPtr WindowHandle => SMProcess.Memory.Read<IntPtr>(new IntPtr(TContentWdwPtr.Read<int>() + Core.Natives.Control.HandleOffset));

    #endregion
  }
}
