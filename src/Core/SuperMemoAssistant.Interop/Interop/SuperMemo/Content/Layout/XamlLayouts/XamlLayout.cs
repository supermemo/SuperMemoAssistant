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
// Created On:   2019/01/18 13:25
// Modified On:  2019/01/18 20:51
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Windows;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Interop.SuperMemo.Content.Layout.XamlControls;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Layout.XamlLayouts
{
  [Serializable]
  public class XamlLayout
  {
    #region Properties & Fields - Public

    public string          Name            { get; set; }
    public ContentTypeFlag AcceptedContent { get; set; }
    public string          Xaml            { get; set; }
    public bool            IsDefault       { get; set; }

    #endregion




    #region Methods Impl

    public override string ToString() => Xaml;

    #endregion




    #region Methods

    public string Build(List<ContentBase> contents)
    {
      return Application.Current.Dispatcher.Invoke<string>(
        () =>
        {
          var ctrlGroup = new XamlControlGroup();

          try
          {
            ctrlGroup.LoadXaml(Xaml);

            return ctrlGroup.ToString(contents);
          }
          finally
          {
            ctrlGroup.Unload();
          }
        }
      );
    }

    #endregion
  }
}
