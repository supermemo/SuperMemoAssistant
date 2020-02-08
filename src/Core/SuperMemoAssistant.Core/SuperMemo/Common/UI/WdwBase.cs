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
// Created On:   2019/02/25 22:02
// Modified On:  2019/03/02 03:28
// Modified By:  Alexis

#endregion




using System;
using Process.NET;
using Process.NET.Windows;
using SuperMemoAssistant.Interop.SuperMemo.UI;
using SuperMemoAssistant.SMA;

namespace SuperMemoAssistant.SuperMemo.Common.UI
{
  public abstract class WdwBase : /*UIAutomationBase,*/ MarshalByRefObject, IWdw
  {
    #region Properties & Fields - Non-Public

    private IWindow _window = null;

    protected abstract IntPtr WindowHandle { get; }


    protected IProcess SMProcess => Core.SMA.SMProcess;

    #endregion




    #region Properties & Fields - Public

    public IWindow Window => _window ?? (_window = GetWindow());

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public IntPtr Handle => WindowHandle;
    /// <inheritdoc />
    public bool IsAvailable { get; protected set; }

    #endregion




    #region Methods

    private IWindow GetWindow()
    {
      return new RemoteWindow(SMProcess,
                              WindowHandle);
    }

    #endregion




    #region Methods Abs

    //
    // Inheritance

    public abstract string WindowClass { get; }

    #endregion




    #region Events

    /// <inheritdoc />
    public abstract event Action OnAvailable;

    #endregion
  }
}
