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
// Created On:   2018/05/25 12:55
// Modified On:  2018/05/31 00:24
// Modified By:  Alexis

#endregion




using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FlaUI.Core.AutomationElements;
using SuperMemoAssistant.Interop.SuperMemo.UI;
using SuperMemoAssistant.Sys.IO.Devices;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.UI
{
  public abstract class MenuBase : IMenu
  {
    #region Properties & Fields - Non-Public

    protected abstract Key Key { get; }

    #endregion




    #region Constructors

    protected MenuBase(WdwBase parent, AutomationElement menu = null)
    {
      Menu   = menu;
      Parent = parent;
    }

    protected MenuBase(WdwBase parent, Func<AutomationElement, bool> filter)
    {
      Parent = parent;

      WaitTask = Task.Run(() =>
      {
        var (success, ae) = Parent.WaitElementAddedAsync(filter, 500).Result;

        if (success == false)
          return false;

        Menu = ae;
        OnMenuSet();

        return true;
      });
    }

    #endregion




    #region Properties & Fields - Public

    protected Task<bool> Awaiter => WaitTask;

    #endregion




    #region Methods

    public virtual Keys GetKeys()
    {
      if (SubMenu != null)
      {
        Keys subMenuKeys = SubMenu.GetKeys();

        return (Key, Awaiter) + subMenuKeys;
      }

      return new Keys(Key);
    }

    protected T Expand<T>(T subMenu)
      where T : MenuBase
    {
      SubMenu = subMenu;

      return subMenu;
    }

    protected virtual void OnMenuSet() { }

    #endregion




    #region Properties & Fields

    protected WdwBase           Parent   { get; }
    protected AutomationElement Menu     { get; set; }
    protected MenuBase          SubMenu  { get; set; }
    protected Task<bool>        WaitTask { get; }

    #endregion
  }
}
