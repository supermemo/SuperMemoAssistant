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
// Modified On:  2019/01/19 00:40
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

    protected abstract IntPtr WindowHandle { get; }

    #endregion




    #region Constructors

    protected WdwBase()
    {
      SMA.Instance.OnSMStartedEvent += OnSMStarted;
      SMA.Instance.OnSMStoppedEvent += OnSMStopped;
    }

    public virtual void Dispose() { }

    #endregion




    #region Properties & Fields - Public

    public IProcess SMProcess { get; set; }

    #endregion




    #region Properties Impl - Public

    public IWindow Window => _window ?? (_window = GetWindow());

    #endregion




    #region Methods

    private IWindow GetWindow()
    {
      return new RemoteWindow(SMProcess,
                              WindowHandle);
    }

    private void OnSMStarted(object        sender,
                             SMProcessArgs e)
    {
      SMProcess = e.Process;
    }

    private void OnSMStopped(object        sender,
                             SMProcessArgs e) { }

    #endregion




    #region Methods Abs

    //
    // Inheritance

    public abstract string WindowClass { get; }

    #endregion
  }
}
