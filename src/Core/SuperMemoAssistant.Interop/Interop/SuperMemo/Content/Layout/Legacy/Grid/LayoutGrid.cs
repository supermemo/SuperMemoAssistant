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
// Modified On:  2019/01/17 18:22
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Drawing;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Interop.SuperMemo.Content.Layout.Legacy.Auto;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Layout.Legacy.Grid
{
  public partial class LayoutGrid : LayoutBase
  {
    #region Properties & Fields - Non-Public

    //protected Dictionary<Index2D, GridContent> AssignedIndexBlockMap { get; } = new Dictionary<Index2D, GridContent>();

    protected HashSet<(ContentBase content, bool allowPlacementInChild)> UnassignedContents { get; } = new HashSet<(ContentBase, bool)>();

    protected GridContent[,] Grid        { get; set; }
    protected VectorData[]  RowsData    { get; set; }
    protected VectorData[]  ColumnsData { get; set; }

    protected List<VectorSize> Columns { get; } = new List<VectorSize>();
    protected List<VectorSize> Rows    { get; } = new List<VectorSize>();

    protected int ColumnCount => Columns.Count;
    protected int RowCount    => Rows.Count;


    protected GridContent this[Index2D idx] => Grid[idx.X, idx.Y];

    #endregion




    #region Constructors

    public LayoutGrid(ContentTypeFlag    acceptedContents,
                      Action<LayoutGrid> buildMethod = null)
      : this(acceptedContents,
             new VectorSize[] { "*" },
             new VectorSize[] { "*" },
             buildMethod) { }

    public LayoutGrid(ContentTypeFlag    acceptedContents,
                      VectorSize[]       rows,
                      VectorSize[]       columns,
                      Action<LayoutGrid> buildMethod = null)
      : base(acceptedContents)
    {
      if (rows == null)
        throw new NullReferenceException(nameof(rows));

      if (columns == null)
        throw new NullReferenceException(nameof(columns));

      Rows.AddRange(rows);
      Columns.AddRange(columns);

      InitializeGrid();

      buildMethod?.Invoke(this);
    }

    #endregion




    #region Properties Impl - Public

    public override Size MinSize { get; }

    #endregion




    #region Methods

    public LayoutGrid AddContent(ContentBase content,
                                 bool        allowPlacementInChild = true)
    {
      UnassignedContents.Add((content, allowPlacementInChild));

      return this;
    }

    public LayoutGrid AddContent(ContentBase content,
                                 int         row,
                                 int         column,
                                 int         rowSpan    = 1,
                                 int         columnSpan = 1)
    {
      return AddContent(content, (row, column));
    }

    public LayoutGrid AddContent(ContentBase content,
                                 Index2D     idx,
                                 int         rowSpan    = 1,
                                 int         columnSpan = 1)
    {
      // RebaseIndex
      ThrowIfInvalid(idx);

      var gridContent = new GridContent(
        content,
        idx,
        new GridSize(content.Size.Width, columnSpan),
        new GridSize(content.Size.Height, rowSpan));

      UpdateGrid(gridContent, idx);

      return this;
    }

    public LayoutGrid AddGrid(ContentTypeFlag    acceptedContents,
                              int                row,
                              int                column,
                              int                rowSpan,
                              int                columnSpan,
                              VectorSize[]       rows,
                              VectorSize[]       columns,
                              Action<LayoutGrid> buildMethod = null)
    {
      return AddGrid(acceptedContents,
                     (row, column),
                     rowSpan, columnSpan,
                     rows, columns,
                     buildMethod);
    }

    public LayoutGrid AddGrid(ContentTypeFlag    acceptedContents,
                              Index2D            idx,
                              int                rowSpan,
                              int                columnSpan,
                              VectorSize[]       rows,
                              VectorSize[]       columns,
                              Action<LayoutGrid> buildMethod = null)
    {
      ThrowIfInvalid(idx);

      var container = new LayoutGrid(
        acceptedContents,
        rows, columns,
        buildMethod);
      var gridContent = new GridContent(
        container,
        idx,
        new GridSize(0, columnSpan),
        new GridSize(0, rowSpan));

      UpdateGrid(gridContent, idx);

      return this;
    }

    public LayoutGrid AddAuto(ContentTypeFlag    acceptedContents,
                              int                row,
                              int                column,
                              int                rowSpan,
                              int                columnSpan,
                              Action<LayoutAuto> buildMethod = null)
    {
      return AddAuto(acceptedContents,
                     (row, column),
                     rowSpan, columnSpan,
                     buildMethod);
    }

    public LayoutGrid AddAuto(ContentTypeFlag    acceptedContents,
                              Index2D            idx,
                              int                rowSpan,
                              int                columnSpan,
                              Action<LayoutAuto> buildMethod = null)
    {
      ThrowIfInvalid(idx);

      var container = new LayoutAuto(
        acceptedContents,
        buildMethod);
      var gridContent = new GridContent(
        container,
        idx,
        new GridSize(0, columnSpan),
        new GridSize(0, rowSpan));

      UpdateGrid(gridContent, idx);

      return this;
    }

    private void ThrowIfInvalid(Index2D idx)
    {
      if (idx.X >= Columns.Count)
        throw new ArgumentException($"Invalid column index {idx.X}, of total count {Columns.Count}");

      if (idx.Y >= Rows.Count)
        throw new ArgumentException($"Invalid row index {idx.Y}, of total count {Rows.Count}");

      if (this[idx] != null)
        throw new ArgumentException($"Block {idx} isn't empty.");
    }

    #endregion
  }
}
