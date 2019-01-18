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




using System;
using System.Drawing;
using System.IO;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

// ReSharper disable LocalizableElement

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Components.Builders
{
  public class ComponentHtmlBuilder : IComponentHtml
  {
    #region Constants & Statics

    public static readonly Size MinSize = new Size(1200,
                                                   1200);

    #endregion




    #region Constructors

    public ComponentHtmlBuilder(int     id,
                                string  text,
                                int     left,
                                int     top,
                                int     width,
                                int     height,
                                AtFlags displayAt)
    {
      Id           = id;
      Left         = (short)left;
      Top          = (short)top;
      Width        = (short)width;
      Height       = (short)height;
      DisplayAt    = displayAt;
      TextFilePath = WriteToFile(text);
    }

    #endregion




    #region Properties & Fields - Public

    public string TextFilePath { get; }

    public int Id { get; }

    #endregion




    #region Properties Impl - Public

    public short   Left      { get; set; }
    public short   Top       { get; set; }
    public short   Width     { get; set; }
    public short   Height    { get; set; }
    public AtFlags DisplayAt { get; set; }

    public IText Text       => throw new NotImplementedException();
    public bool  IsFullHtml { get; set; } = true;

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      return $@"Begin Component #{Id + 1}
Type=HTML
Cors=({Left},{Top},{Width},{Height})
DisplayAt={(int)DisplayAt}
Hyperlink=0
HTMName=htm
HTMFile={TextFilePath}
TestElement=0
ReadOnly=0
FullHTML={(IsFullHtml ? 1 : 0)}
Style=0
End Component #{Id + 1}";
    }

    #endregion




    #region Methods

    private string WriteToFile(string text)
    {
      var filePath = Path.Combine(Path.GetTempPath(),
                                  $"sm_element_{Id}.htm");

      File.WriteAllText(filePath,
                        text + "\r\n<span />");

      return filePath;
    }

    #endregion
  }
}
