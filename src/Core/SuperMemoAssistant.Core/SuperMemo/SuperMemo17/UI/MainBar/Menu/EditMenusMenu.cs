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
// Created On:   2018/05/25 13:49
// Modified On:  2018/05/31 00:23
// Modified By:  Alexis

#endregion




using System.Windows.Input;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.UI.MainBar.Menu;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.UI.MainBar.Menu
{
  public class EditMenusMenu : MenuNode, IEditMenusMenu
  {
    #region Constructors

    public EditMenusMenu(WdwBase parent)
      : base(parent, FilterType.ClassName, SMConst.UI.MainMenuItemClassName, Key.M) { }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public void Elements()
    {
      ChildKey = Key.E;
    }
    
    /// <inheritdoc />
    public void Components()
    {
      ChildKey = Key.O;
    }
    
    /// <inheritdoc />
    public void Contents()
    {
      ChildKey = Key.C;
    }
    
    /// <inheritdoc />
    public void Browser()
    {
      ChildKey = Key.B;
    }
    
    /// <inheritdoc />
    public void Registry()
    {
      ChildKey = Key.R;
    }

    #endregion
  }
}
