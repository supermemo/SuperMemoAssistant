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
// Modified On:  2019/01/17 20:48
// Modified By:  Alexis

#endregion




using System;
using System.Drawing;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Layout.Legacy.Grid
{
  public class GridContent : LayoutBase
  {
    #region Constructors

    public GridContent(Index2D  idx,
                       GridSize widthDefinition,
                       GridSize heightDefinition)
      : base(ContentTypeFlag.All)
    {
      Index     = idx;
      WidthDef  = widthDefinition;
      HeightDef = heightDefinition;
    }

    public GridContent(ContentBase content,
                       Index2D     idx,
                       GridSize    widthDefinition,
                       GridSize    heightDefinition)
      : this(idx, widthDefinition, heightDefinition)
    {
      Content = content;

      MinSize = ComputeMinSize();
    }

    public GridContent(LayoutBase container,
                       Index2D    idx,
                       GridSize   widthDefinition,
                       GridSize   heightDefinition)
      : this(idx, widthDefinition, heightDefinition)
    {
      Container = container;

      MinSize = ComputeMinSize();
    }

    #endregion




    #region Properties & Fields - Public

    public Index2D  Index     { get; }
    public GridSize WidthDef  { get; }
    public GridSize HeightDef { get; }
    public bool     HasSize   => MinSize.IsEmpty == false;

    public LayoutBase  Container { get; }
    public ContentBase Content   { get; }
    public bool        IsContent => Content != null;

    #endregion




    #region Properties Impl - Public

    public override Size MinSize { get; }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override void InitializeLayout() { }

    /// <inheritdoc />
    public override void CalculateLayout(Rectangle cors) { }

    #endregion




    #region Methods

    private Size ComputeMinSize()
    {
      int width = IsContent
        ? Content.MinSize.Width
        : 0;
      int height = IsContent
        ? Content.MinSize.Height
        : 0;

      width  = Math.Max(width, WidthDef);
      height = Math.Max(height, HeightDef);

      return new Size(width,
                      height);
    }

    #endregion
  }
}
