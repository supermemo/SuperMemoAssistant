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
// Created On:   2018/05/27 22:53
// Modified On:  2018/05/31 00:23
// Modified By:  Alexis

#endregion




using System;
using System.Windows.Input;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using SuperMemoAssistant.Sys.IO.Devices;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.UI
{
  public class MenuNode : MenuBase
  {
    #region Properties & Fields - Non-Public

    protected Key ChildKey { get; set; }

    protected override Key Key { get; }

    #endregion




    #region Constructors

    public MenuNode(
      WdwBase    parent,
      FilterType filterType,
      string     filterValue,
      Key        key)
      : base(parent, FilterOn(filterType, filterValue))
    {
      Key = key;
    }

    #endregion




    #region Methods Impl

    public override Keys GetKeys()
    {
      if (SubMenu != null)
        return base.GetKeys();

      return new Keys((Key, Awaiter), (ChildKey, null));
    }

    #endregion




    #region Methods

    private static Func<AutomationElement, bool> FilterOn(FilterType filterType, string value)
    {
      switch (filterType)
      {
        case FilterType.Name:
          return FilterOnName(value);

        case FilterType.ClassName:
          return FilterOnClassName(value);

        default:
          throw new ArgumentException($"Invalid FilterType {filterType}");
      }
    }

    private static Func<AutomationElement, bool> FilterOnName(string name)
    {
      bool FilterFunc(AutomationElement ae)
      {
        return ae.ControlType == ControlType.Menu
          && name.Equals(ae.Name);
      }

      return FilterFunc;
    }

    private static Func<AutomationElement, bool> FilterOnClassName(string className)
    {
      bool FilterFunc(AutomationElement ae)
      {
        return ae.ControlType == ControlType.Menu
          && className.Equals(ae.ClassName);
      }

      return FilterFunc;
    }

    #endregion




    #region Enums

    public enum FilterType
    {
      Name,
      ClassName,
    }

    #endregion
  }
}
