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
// Created On:   2019/01/16 14:53
// Modified On:  2019/01/16 20:05
// Modified By:  Alexis

#endregion




using System.Drawing;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Components.Builders
{
  public class ComponentImageBuilder : IComponentImage
  {
    #region Constants & Statics

    public static readonly Size MinSize = new Size(1200,
                                                   1200);

    #endregion




    #region Constructors

    public ComponentImageBuilder(int     id,
                                 IImage  img,
                                 int     left,
                                 int     top,
                                 int     width,
                                 int     height,
                                 AtFlags displayAt)
    {
      Id        = id;
      Image     = img;
      Left      = (short)left;
      Top       = (short)top;
      Width     = (short)width;
      Height    = (short)height;
      DisplayAt = displayAt;
    }

    #endregion




    #region Properties & Fields - Public

    public int Id { get; }

    #endregion




    #region Properties Impl - Public

    public short   Left      { get; set; }
    public short   Top       { get; set; }
    public short   Width     { get; set; }
    public short   Height    { get; set; }
    public AtFlags DisplayAt { get; set; }


    public IImage           Image   { get; set; }
    public ImageStretchMode Stretch { get; set; } = ImageStretchMode.Proportional;

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      return $@"Begin Component #{Id + 1}
Type=Image
Cors=({Left},{Top},{Width},{Height})
DisplayAt={(int)DisplayAt}
Hyperlink=0
ImageName={Image.Name}
ImageFile={Image.GetFilePath()}
Stretch={(int)Stretch}
ClickPlay=0
TestElement=0
Transparent=0
Zoom=[0,0,0,0]
End Component #{Id + 1}";
    }

    #endregion
  }
}
