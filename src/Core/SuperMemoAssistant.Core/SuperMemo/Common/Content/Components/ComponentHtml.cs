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
// Modified On:  2020/04/09 15:39
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.SuperMemo.Common.Content.Components
{
  using Interop.SuperMemo.Content.Components;
  using Interop.SuperMemo.Content.Models;
  using Interop.SuperMemo.Registry.Members;
  using SMA;
  using SuperMemo17.Files;

  internal class ComponentHtml : ComponentBase, IComponentHtml
  {
    #region Properties & Fields - Non-Public

    public int TextId     { get; set; }
    public int ColorRed   { get; set; }
    public int ColorGreen { get; set; }
    public int ColorBlue  { get; set; }

    #endregion




    #region Constructors

    public ComponentHtml(ref InfComponentsHtml17 comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt)
    {
      TextId     = SetValue(comp.registryId, nameof(TextId));
      IsFullHtml = SetValue(comp.isFullHtml != 0, nameof(IsFullHtml));
    }

    #endregion




    #region Properties Impl - Public

    public IText Text       => Core.SM.Registry.Text?[TextId];
    public bool  IsFullHtml { get; set; }

    #endregion




    #region Methods

    public void Update(ref InfComponentsHtml17 comp)
    {
      ComponentFieldFlags flags = ComponentFieldFlags.None;

      TextId     = SetValue(TextId, comp.registryId, nameof(TextId), ref flags);
      IsFullHtml = SetValue(IsFullHtml, comp.isFullHtml != 0, nameof(IsFullHtml), ref flags);

      base.Update(
        comp.left, comp.top,
        comp.width, comp.height,
        (AtFlags)comp.displayAt,
        flags
      );
    }

    #endregion
  }
}
