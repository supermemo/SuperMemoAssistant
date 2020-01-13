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
// Created On:   2019/02/26 23:18
// Modified On:  2019/03/01 21:21
// Modified By:  Alexis

#endregion




using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using HtmlAgilityPack;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

// ReSharper disable LocalizableElement

namespace SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlControls
{
  /// <summary>Interaction logic for XamlControlHtml.xaml</summary>
  public partial class XamlControlHtml : XamlControlBase, IComponentHtml, INotifyPropertyChanged
  {
    #region Constants & Statics

    public static readonly Size MinSize = new Size(1200,
                                                   1200);

    #endregion




    #region Constructors

    public XamlControlHtml(int     id,
                           string  html,
                           AtFlags displayAt)
    {
      Id        = id;
      DisplayAt = displayAt;
      Html      = html;

      if (DesignerProperties.GetIsInDesignMode(this) == false && id != int.MinValue)
        TextFilePath = WriteToFile(html);

      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public string Html         { get; set; }
    public string TextFilePath { get; }
    public int    Id           { get; }

    #endregion




    #region Properties Impl - Public

    public override AtFlags DisplayAt { get; }

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

    private string WriteToFile(string html)
    {
      var filePath = Path.Combine(Path.GetTempPath(),
                                  $"sm_element_{Id}.htm");

      html = HtmlEntity.Entitize(html);

      File.WriteAllText(filePath,
                        html + "\r\n<span />",
                        Encoding.UTF8);

      return filePath;
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
