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
// Created On:   2019/01/01 18:04
// Modified On:  2019/01/01 18:21
// Modified By:  Alexis

#endregion




using System;
using System.Globalization;
using System.Windows.Data;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;

namespace SuperMemoAssistant.Sys.Windows.Data
{
  public class ElementIconConverter : IValueConverter
  {
    #region Constants & Statics

    private const string BasePath = "pack://application:,,,/SuperMemoAssistant.Interop;component/Resources/";

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public object Convert(object      value,
                          Type        targetType,
                          object      parameter,
                          CultureInfo culture)
    {
      if (value is ElementType == false)
        return string.Empty;

      var type = (ElementType)value;

      switch (type)
      {
        case ElementType.Topic:
          return BasePath + "topic_icon.jpg";

        case ElementType.Item:
          return BasePath + "item_icon.jpg";

        case ElementType.ConceptGroup:
          return BasePath + "concept_icon.jpg";
      }

      return string.Empty;
    }

    /// <inheritdoc />
    public object ConvertBack(object      value,
                              Type        targetType,
                              object      parameter,
                              CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
