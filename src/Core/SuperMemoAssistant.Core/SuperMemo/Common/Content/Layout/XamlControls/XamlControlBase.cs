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
// Created On:   2019/01/18 03:07
// Modified On:  2019/01/18 03:26
// Modified By:  Alexis

#endregion




using System.Drawing;
using System.Windows.Controls;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using Point = System.Windows.Point;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlControls
{
  public abstract class XamlControlBase : UserControl, IComponent
  {
    #region Constructors

    // ReSharper disable once PublicConstructorInAbstractClass
    public XamlControlBase()
    {
      LayoutUpdated += OnLayoutUpdated;
    }

    #endregion




    #region Properties & Fields - Public

    public Rectangle Bounds { get; set; }

    #endregion




    #region Properties Impl - Public

    public     short Left   => (short)Bounds.Left;
    public     short Top    => (short)Bounds.Top;
    public new short Width  => (short)Bounds.Width;
    public new short Height => (short)Bounds.Height;

    #endregion




    #region Methods

    private void OnLayoutUpdated(object           sender,
                                 System.EventArgs e)
    {
      var root = GetRoot();

      if (root == null)
        return;

      var totalWidth  = root.ActualWidth;
      var totalHeight = root.ActualHeight;

      var xScale = 10000.0 / totalWidth;
      var yScale = 10000.0 / totalHeight;

      var transform = TransformToAncestor(root);
      var topLeft   = transform.Transform(new Point(0, 0));

      Bounds = new Rectangle(
        (int)(topLeft.X * xScale),
        (int)(topLeft.Y * yScale),
        (int)(ActualWidth * xScale),
        (int)(ActualHeight * yScale));
    }


    public XamlControlGroup GetRoot() => this.FindParent<XamlControlGroup>();

    #endregion




    #region Methods Abs

    public abstract AtFlags DisplayAt { get; }

    #endregion
  }
}
