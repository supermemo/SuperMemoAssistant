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
// Created On:   2018/05/25 12:50
// Modified On:  2018/05/30 23:37
// Modified By:  Alexis

#endregion




using System;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.UI.MainBar;
using SuperMemoAssistant.Interop.SuperMemo.UI.MainBar.Menu;
using SuperMemoAssistant.SuperMemo.SuperMemo17.UI.MainBar.Menu;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.UI.MainBar
{
  [InitOnLoad]
  public class MainBarWdw : WdwBase, IMainBarWdw
  {
    #region Constants & Statics

    public static MainBarWdw Instance { get; } = new MainBarWdw();

    #endregion




    #region Constructors

    protected MainBarWdw() { }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override string WindowClass => SMConst.UI.SMMainClassName;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public Task<bool> OpenMenu(Action<IMainMenu> action)
    {
      return ExecuteMenuAction(new MainMenu(this), action);
    }

    #endregion
  }
}
