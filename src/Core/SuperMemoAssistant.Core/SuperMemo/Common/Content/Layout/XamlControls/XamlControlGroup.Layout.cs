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
// Created On:   2019/02/27 02:33
// Modified On:  2019/03/01 23:50
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.SuperMemo.Common.Content.Content;
using SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlLayouts;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlControls
{
  public partial class XamlControlGroup
  {
    #region Constants & Statics

    public static readonly Size SuperMemoFrameSize = new Size(10000, 10000);

    #endregion




    #region Properties & Fields - Non-Public

    //private List<CollapsableGrid> _collapsableGrids = new List<CollapsableGrid>();
    private readonly List<CollapsableGridAttachedProperty> _collapsableGrids = new List<CollapsableGridAttachedProperty>();

    #endregion




    #region Properties & Fields - Public

    public bool            IsValid         { get; private set; } = true;
    public ContentTypeFlag AcceptedContent { get; private set; }

    #endregion




    #region Methods

    public bool SetXamlLayout(XamlLayout xamlLayout)
    {
      Reset();

      var rootObject = xamlLayout.ParseLayout(out var ex);

      if (ex != null || !(rootObject is UIElement rootUIElement))
      {
        LogTo.Warning(ex, $"Xaml layout {xamlLayout.Name} is invalid");

        return IsValid = false;
      }

      Content = rootUIElement;

      CalculateLayout();

      FindContentContainers();
      ComputeContentPanelMap();

      ValidateAcceptedContents();
      SetupContentColors();

      ComputeAcceptedContent();

      GenerateDemoContent();

      return IsValid;
    }

    public void Reset()
    {
      ClearContent();

      _panelAcceptedContents.Clear();
      _contentPanelMap.Clear();

      AcceptedContent = ContentTypeFlag.None;
      IsValid         = true;

      _collapsableGrids.Clear();

      Content = null;
    }

    private void CalculateLayout()
    {
      if (_displayMode == false)
      {
        Measure(SuperMemoFrameSize);
        Arrange(new Rect(SuperMemoFrameSize));
      }
      else
      {
        Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Arrange(new Rect(RenderSize));
        //ApplyTemplate();
      }

      UpdateLayout();
    }

    private void FindContentContainers()
    {
      foreach (var child in this.EnumerateChildren())
      {
        if (!(child is Panel panel))
          continue;

        //if (panel is CollapsableGrid collapsableGrid)
        //  _collapsableGrids.Add(collapsableGrid);

        // Collapsable grids
        var collapsableGrid = child.ReadLocalValue(Grids.CollapsableProperty);

        if (collapsableGrid != DependencyProperty.UnsetValue && collapsableGrid.GetType() == Grids.CollapsableProperty.PropertyType)
          _collapsableGrids.Add((CollapsableGridAttachedProperty)collapsableGrid);

        // Accepted contents
        var acceptedContent = child.ReadLocalValue(Panels.AcceptedContentProperty);

        if (acceptedContent == DependencyProperty.UnsetValue || acceptedContent.GetType() != Panels.AcceptedContentProperty.PropertyType)
          continue;

        _panelAcceptedContents.Add((panel, (ContentTypeFlag)acceptedContent));
      }

      if (_panelAcceptedContents.Count == 0)
        IsValid = false;
    }

    private void ComputeContentPanelMap()
    {
      foreach (var pc in _panelAcceptedContents)
        for (int cf = (int)pc.content, shift = 0;
             cf > 0;
             cf >>= 1, shift++)
        {
          var c = cf & 0x0001;

          if (c > 0)
            _contentPanelMap[(ContentTypeFlag)(c << shift)] = pc.panel;
        }
    }

    private void ValidateAcceptedContents()
    {
      // Check for duplicates or overlaps
      for (int i = 0; i < _panelAcceptedContents.Count; i++)
      for (int j = 0; j < _panelAcceptedContents.Count; j++)
      {
        if (i == j)
          continue;

        var (p1, ac1) = _panelAcceptedContents[i];
        var (p2, ac2) = _panelAcceptedContents[j];

        if (ac1 == ContentTypeFlag.None || ac2 == ContentTypeFlag.None)
          continue;

        if ((ac1 & ac2) == ContentTypeFlag.None)
          continue;

        p1.Background = p2.Background = Brushes.Red;
        IsValid       = false;
      }
    }

    private void SetupContentColors()
    {
      foreach (var pc in _panelAcceptedContents)
      {
        if (pc.panel.Background is SolidColorBrush)
          continue;

        pc.Item1.Background = pc.content.GetColorBrush();
      }
    }

    private void ComputeAcceptedContent()
    {
      if (_panelAcceptedContents.Count == 0)
        return;

      AcceptedContent = _panelAcceptedContents
                        .Select(pc => pc.content)
                        .Aggregate((c1,
                                    c2) => c1 | c2);
    }

    #endregion
  }
}
