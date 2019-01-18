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
// Created On:   2019/01/17 14:35
// Modified On:  2019/01/17 20:54
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Layout.Legacy.Grid
{
  public partial class LayoutGrid
  {
    #region Methods Impl

    public override void InitializeLayout() { }

    public override void CalculateLayout(Rectangle cors)
    {
      RowsData.ForEach(d => d.Reset());
      ColumnsData.ForEach(d => d.Reset());

      HashSet<GridContent> visitedContent = new HashSet<GridContent>();

      for (int i = 0; i < RowCount; i++)
      for (int j = 0; j < ColumnCount; j++)
      {
        var content = Grid[i, j];

        // If no content, create an Auto container
        if (content == null)
        {
          AddAuto(AcceptedContents,
                  (i, j),
                  1, 1);
          content = Grid[i, j];

          if (content == null)
            throw new InvalidOperationException("Content should not be null.");
        }

        var visited    = visitedContent.Contains(content);
        var rowSize    = Rows[i];
        var columnSize = Columns[j];

        // Compute new free size for row + column & Increment counters
        if (columnSize.IsFill)
        {
          if (visited == false)
            RowsData[i].UsedSize += content.MinSize.Width;
        }
        else
        {
          RowsData[i].FillBlockCount++;
        }

        if (rowSize.IsFill)
        {
          if (visited == false)
            ColumnsData[j].UsedSize += content.MinSize.Height;
        }
        else
        {
          ColumnsData[j].FillBlockCount++;
        }

        ColumnsData[j].BlockCount++;
        RowsData[i].BlockCount++;

        visitedContent.Add(content);
      }
    }

    #endregion




    #region Methods

    private void InitializeGrid()
    {
      Grid        = new GridContent[RowCount, ColumnCount];
      RowsData    = Enumerable.Repeat(new VectorData(), RowCount).ToArray();
      ColumnsData = Enumerable.Repeat(new VectorData(), ColumnCount).ToArray();
    }

    private void UpdateGrid(GridContent content,
                            Index2D     idx)
    {
      var widthDef  = content.WidthDef;
      var heightDef = content.HeightDef;

      // Validate Spans
      if (idx.X + widthDef.Span > ColumnCount)
        throw new ArgumentException("Column span is out of bound");

      if (idx.Y + heightDef.Span > RowCount)
        throw new ArgumentException("Row span is out of bound");

      // Insert content
      for (int i = idx.X; i < idx.X + widthDef.Span; i++)
      for (int j = idx.Y; j < idx.Y + heightDef.Span; j++)
      {
        // TODO: Implement auto for containers
        if (content.IsContent == false &&
          (Columns[i].IsAuto || Rows[j].IsAuto))
          throw new NotImplementedException("Auto not implemented for containers");

        Grid[i, j] = content;
      }
    }

    public string Build(List<ContentBase> contents)
    {
      if (Root)
      {
        // TODO
      }
    }

    protected void GetCors(ContentBase content,
                           int         idxComp,
                           int         totalCompCount,
                           out int     compLeft,
                           out int     compTop,
                           out int     compWidth,
                           out int     compHeight)
    {
      if (content.Cors != null)
      {
        compLeft   = content.Cors.Value.Left;
        compTop    = content.Cors.Value.Top;
        compWidth  = content.Cors.Value.Width;
        compHeight = content.Cors.Value.Height;
        return;
      }

      if (totalCompCount == 1)
      {
        compLeft   = Left;
        compTop    = Top;
        compWidth  = Width;
        compHeight = Height;
        return;
      }

      if (totalCompCount == 2)
      {
        int comp2Height = (int)(totalHeight / 2.0);

        compLeft   = left;
        compTop    = top + (idxComp == 0 ? 0 : comp2Height);
        compWidth  = totalWidth;
        compHeight = comp2Height;
        return;
      }

      double nbRowAndCol = Math.Floor(Math.Log(totalCompCount,
                                               2));
      int compRow = (int)Math.Floor(idxComp / nbRowAndCol);
      int compCol = idxComp % (int)nbRowAndCol;

      compWidth  = (int)(totalWidth / nbRowAndCol);
      compHeight = (int)(totalHeight / nbRowAndCol);

      compLeft = left + compCol * compWidth;
      compTop  = top + compRow * compHeight;
    }

    #endregion
  }
}
