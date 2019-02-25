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
// Created On:   2019/01/16 15:55
// Modified On:  2019/01/19 00:43
// Modified By:  Alexis

#endregion




using System;
using System.Windows;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using Size = System.Drawing.Size;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Contents
{
  [Serializable]
  public class ImageContent : ContentBase
  {
    #region Constructors

    public ImageContent(int                 registryId,
                        ImageStretchMode    stretchType = ImageStretchMode.Proportional,
                        AtFlags             displayAt   = AtFlags.All,
                        VerticalAlignment   vAlignment  = VerticalAlignment.Stretch,
                        HorizontalAlignment hAlignment  = HorizontalAlignment.Stretch,
                        Size                size        = default)
      : base(displayAt,
             vAlignment,
             hAlignment,
             size)
    {
      RegistryId  = registryId;
      StretchType = stretchType;
    }

    #endregion




    #region Properties & Fields - Public

    public int RegistryId { get; set; }

    public ImageStretchMode StretchType { get; set; }

    #endregion




    #region Properties Impl - Public

    public override ContentTypeFlag ContentType => ContentTypeFlag.Image;
    // TODO: Use image size
    public override Size MinCompSize => new Size(1200, 1200);

    #endregion
  }
}
