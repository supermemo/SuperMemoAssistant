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
// Created On:   2019/01/18 01:57
// Modified On:  2019/01/18 02:23
// Modified By:  Alexis

#endregion




using System;
using System.Windows;
using System.Windows.Controls;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlControls
{
  public class AutoGrid : Grid
  {
    #region Methods Impl

    protected override void OnVisualChildrenChanged(DependencyObject visualAdded,
                                                    DependencyObject visualRemoved)
    {
      base.OnVisualChildrenChanged(visualAdded, visualRemoved);

      if (visualRemoved != null)
        return;

      ColumnDefinitions.Clear();
      RowDefinitions.Clear();

      var count = Children.Count;

      if (count == 0)
        return;

      GetSpecs(out var colCount, out var rowCount);

      for (int i = 0; i < colCount; i++)
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) });

      for (int i = 0; i < rowCount; i++)
        RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.0, GridUnitType.Star) });

      int idx = 0;

      for (int i = 0; i < colCount; i++)
      for (int j = 0; j < rowCount && idx < count; j++, idx++)
      {
        SetColumn(Children[idx], i);
        SetRow(Children[idx], j);
      }
    }

    #endregion




    #region Methods

    private void GetSpecs(out int colCount,
                          out int rowCount)
    {
      var count = Children.Count;

      if (count == 1)
      {
        colCount = 1;
        rowCount = 1;
        return;
      }

      if (count == 2)
      {
        if (ActualWidth > ActualHeight)
        {
          colCount = 2;
          rowCount = 1;
        }

        else
        {
          colCount = 1;
          rowCount = 2;
        }

        return;
      }

      colCount = rowCount = (int)Math.Ceiling(Math.Sqrt(count));
    }

    #endregion
  }
}
