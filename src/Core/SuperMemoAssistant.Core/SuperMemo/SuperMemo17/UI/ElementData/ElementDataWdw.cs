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
// Created On:   2018/05/24 13:13
// Modified On:  2018/05/30 23:52
// Modified By:  Alexis

#endregion




using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.UI.ElementData;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.UI.ElementData
{
  [InitOnLoad]
  public class ElementDataWdw : WdwBase, IElementDataWdw
  {
    #region Constants & Statics

    protected static Regex RE_TitleElementId { get; } = new Regex("[^ ]+ #([\\d]+):.*", RegexOptions.Compiled);

    public static ElementDataWdw Instance { get; } = new ElementDataWdw();

    #endregion




    #region Constructors

    protected ElementDataWdw()
    {
      CurrentElementId = -1;
    }

    #endregion




    #region Properties Impl - Public

    public override string WindowClass => SMConst.UI.ElementDataWindowClassName;

    public int      CurrentElementId { get; set; }
    public IElement CurrentElement   => SMA.Instance.Registry.Element?[CurrentElementId];

    #endregion




    #region Methods Impl

    protected override void OnWindowOpened(AutomationElement elem, EventId eventId)
    {
      base.OnWindowOpened(elem, eventId);

      Window.RegisterPropertyChangedEvent(
        TreeScope.Element,
        (ae, p, v) => OnWindowPropertyChanged(ae, null),
        UIAuto.PropertyLibrary.Element.Name
      );
    }

    #endregion




    #region Methods

    protected void OnWindowPropertyChanged(AutomationElement ae, EventId _)
    {
#if DEBUG
      Debug.WriteLine("[ElDataWdw] title: {0}", new object[] { ae?.Name });
#endif

      string title = ae?.Name;

      if (title == null)
        return;

      var match = RE_TitleElementId.Match(title);

      if (match.Success == false)
        return;

      CurrentElementId = int.Parse(match.Groups[1].Value);

      try
      {
        OnElementChanged?.Invoke(new SMElementArgs(SMA.Instance, CurrentElement));
      }
      catch (Exception ex) { }

#if DEBUG
      Debug.WriteLine("[ElDataWdw] element: {0}", new object[] { CurrentElement?.Title });
#endif
    }

    #endregion




    #region Events

    /// <inheritdoc />
    public event Action<SMElementArgs> OnElementChanged;

    #endregion
  }
}
