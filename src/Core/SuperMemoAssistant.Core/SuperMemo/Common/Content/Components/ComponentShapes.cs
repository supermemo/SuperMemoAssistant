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
// Created On:   2020/03/29 00:20
// Modified On:  2020/04/09 15:38
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.SuperMemo.Common.Content.Components
{
  using Interop.SuperMemo.Content.Components;
  using Interop.SuperMemo.Content.Models;
  using SuperMemo17.Files;

  internal class ComponentShapeEllipse : ComponentBase, IComponentShapeEllipse
  {
    #region Constructors

    public ComponentShapeEllipse(ref InfComponentsShape17 comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt) { }

    #endregion




    #region Methods

    public void Update(ref InfComponentsShape17 comp)
    {
      base.Update(
        comp.left, comp.top,
        comp.width, comp.height,
        (AtFlags)comp.displayAt,
        ComponentFieldFlags.None
      );
    }

    #endregion
  }

  internal class ComponentShapeRectangle : ComponentBase, IComponentShapeRectangle
  {
    #region Constructors

    public ComponentShapeRectangle(ref InfComponentsShape17 comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt) { }

    #endregion




    #region Methods

    public void Update(ref InfComponentsShape17 comp)
    {
      base.Update(
        comp.left, comp.top,
        comp.width, comp.height,
        (AtFlags)comp.displayAt,
        ComponentFieldFlags.None
      );
    }

    #endregion
  }

  internal class ComponentShapeRoundedRectangle : ComponentBase, IComponentShapeRoundedRectangle
  {
    #region Constructors

    public ComponentShapeRoundedRectangle(ref InfComponentsShape17 comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt) { }

    #endregion




    #region Methods

    public void Update(ref InfComponentsShape17 comp)
    {
      base.Update(
        comp.left, comp.top,
        comp.width, comp.height,
        (AtFlags)comp.displayAt,
        ComponentFieldFlags.None
      );
    }

    #endregion
  }
}
