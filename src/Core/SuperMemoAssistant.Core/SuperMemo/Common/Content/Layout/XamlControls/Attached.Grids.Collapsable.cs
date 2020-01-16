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
// Created On:   2019/03/01 23:36
// Modified On:  2019/03/02 00:17
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MoreLinq;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlControls
{
  public class CollapsableGridAttachedProperty
  {
    #region Properties & Fields - Non-Public

    private Grid                   _grid;
    private List<ColumnDefinition> _columnDefinitions;
    private List<RowDefinition>    _rowDefinitions;


    private ColumnDefinition EmptyColumn => new ColumnDefinition
    {
      Width    = GridLength.Auto,
      MinWidth = 0,
      MaxWidth = 0
    };

    private RowDefinition EmptyRow => new RowDefinition
    {
      Height    = GridLength.Auto,
      MinHeight = 0,
      MaxHeight = 0
    };

    #endregion




    #region Constructors

    public CollapsableGridAttachedProperty() { }

    #endregion




    #region Methods

    public void Attach(Grid grid)
    {
      _grid             =  grid;
      _grid.Initialized += OnInitialized;
    }

    public void Refresh()
    {
      bool hasRows = _rowDefinitions.Count > 0;
      bool hasCols = _columnDefinitions.Count > 0;
      int  rCount  = Math.Max(_rowDefinitions.Count, 1);
      int  cCount  = Math.Max(_columnDefinitions.Count, 1);
      var  g       = new List<UIElement>[rCount, cCount];
      var  rc      = MoreEnumerable.Generate(true, b => b).Take(rCount).ToArray();
      var  cc      = MoreEnumerable.Generate(true, b => b).Take(cCount).ToArray();

      for (int i = 0; i < rCount; i++)
      for (int j = 0; j < cCount; j++)
        g[i, j] = new List<UIElement>();

      for (int k = 0; k < _grid.Children.Count; k++)
      {
        var c = _grid.Children[k];
        int i = (int)(c.GetValue(Grid.RowProperty) ?? 0);
        int j = (int)(c.GetValue(Grid.ColumnProperty) ?? 0);

        g[i, j].Add(c);
      }

      for (int i = 0; i < rCount; i++)
      for (int j = 0; j < cCount; j++)
      {
        var hasContent = g[i, j].Any(HasContent);

        rc[i] = rc[i] && hasContent;
        cc[j] = cc[j] && hasContent;
      }

      if (hasRows)
        for (int i = 0; i < rCount; i++)
          _grid.RowDefinitions[i] = rc[i] ? CloneRow(i) : EmptyRow;

      if (hasCols)
        for (int j = 0; j < cCount; j++)
          _grid.ColumnDefinitions[j] = cc[j] ? CloneColumn(j) : EmptyColumn;
    }

    private bool HasContent(UIElement element)
    {
      if (element is XamlControlBase)
        return true;

      if (!(element is Panel panel))
        return false;

      for (int i = 0; i < panel.Children.Count; i++)
      {
        var c = panel.Children[i];

        if (c is Panel childPanel)
        {
          if (HasContent(childPanel))
            return true;
        }

        else if (c is XamlControlBase)
        {
          return true;
        }
      }

      return false;
    }

    private void OnInitialized(object sender, EventArgs e)
    {
      _columnDefinitions = new List<ColumnDefinition>(_grid.ColumnDefinitions.Count);
      _rowDefinitions    = new List<RowDefinition>(_grid.RowDefinitions.Count);

      foreach (var cd in _grid.ColumnDefinitions)
        _columnDefinitions.Add(new ColumnDefinition
        {
          Width           = cd.Width,
          MinWidth        = cd.MinWidth,
          MaxWidth        = cd.MaxWidth,
          SharedSizeGroup = cd.SharedSizeGroup
        });

      foreach (var rd in _grid.RowDefinitions)
        _rowDefinitions.Add(new RowDefinition
        {
          Height          = rd.Height,
          MinHeight       = rd.MinHeight,
          MaxHeight       = rd.MaxHeight,
          SharedSizeGroup = rd.SharedSizeGroup
        });
    }

    private ColumnDefinition CloneColumn(int i)
    {
      var cd = _columnDefinitions[i];

      return new ColumnDefinition
      {
        Width           = cd.Width,
        MinWidth        = cd.MinWidth,
        MaxWidth        = cd.MaxWidth,
        SharedSizeGroup = cd.SharedSizeGroup
      };
    }

    private RowDefinition CloneRow(int i)
    {
      var rd = _rowDefinitions[i];

      return new RowDefinition
      {
        Height          = rd.Height,
        MinHeight       = rd.MinHeight,
        MaxHeight       = rd.MaxHeight,
        SharedSizeGroup = rd.SharedSizeGroup
      };
    }

    public static implicit operator CollapsableGridAttachedProperty(string enable)
    {
      return enable.Equals("true", StringComparison.OrdinalIgnoreCase)
        ? new CollapsableGridAttachedProperty()
        : null;
    }

    #endregion
  }
}
