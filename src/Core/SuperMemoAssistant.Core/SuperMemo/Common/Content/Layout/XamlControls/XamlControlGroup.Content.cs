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
// Created On:   2019/02/27 02:07
// Modified On:  2019/02/27 02:42
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.SuperMemo.Common.Content.Content;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlControls
{
  public partial class XamlControlGroup
  {
    #region Constants & Statics

    private const string ComponentsSkeleton = @"ComponentNo={0}
{1}";

    #endregion




    #region Properties & Fields - Non-Public

    private readonly Dictionary<ContentTypeFlag, Panel> _contentPanelMap = new Dictionary<ContentTypeFlag, Panel>();

    private readonly List<UIElement> _contentUIElements = new List<UIElement>();

    private readonly List<(Panel panel, ContentTypeFlag content)> _panelAcceptedContents = new List<(Panel, ContentTypeFlag)>();

    #endregion




    #region Methods

    public string ToElementDefinition(List<ContentBase> contents)
    {
      if (!IsValid)
        return null;

      AddContent(contents);

      UpdateLayout();

      var ret = string.Format(
        ComponentsSkeleton,
        _contentUIElements.Count,
        string.Join("\n", _contentUIElements.Select(c => c.ToString())));

      ClearContent();

      return ret;
    }

    public void AddContent(List<ContentBase> contents)
    {
      if (!IsValid && _displayMode == false)
        return;

      int id = 0;

      foreach (var content in contents)
      {
        var panel = _contentPanelMap.SafeGet(content.ContentType);

        if (panel == null)
          continue;

        var uiElement = content.ToUIElement(_displayMode ? int.MinValue : id++);

        if (uiElement == null)
          continue;

        panel.Children.Add(uiElement);
        _contentUIElements.Add(uiElement);
      }

      _collapsableGrids.ForEach(cg => cg.Refresh());
    }

    public void ClearContent()
    {
      foreach (var pc in _panelAcceptedContents)
        pc.panel.Children.Clear();

      _contentUIElements.Clear();
    }

    public void GenerateDemoContent()
    {
      ClearContent();

      if (_displayMode)
      {
        var demoContents = new List<ContentBase>();

        for (int i = 0; i < TextContentCount; i++)
          demoContents.Add(
            new TextContent(
              false,
              "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard ...")
          );
        
        for (int i = 0; i < ImageContentCount; i++)
          demoContents.Add(new ImageContent(0));

        for (int i = 0; i < SoundContentCount; i++)
          demoContents.Add(new SoundContent(0, "Sound caption text"));

        AddContent(demoContents);
      }
    }

    #endregion
  }
}
