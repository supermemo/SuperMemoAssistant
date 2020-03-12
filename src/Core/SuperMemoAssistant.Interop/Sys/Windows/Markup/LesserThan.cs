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
// Modified On:  2020/03/11 00:16
// Modified By:  Alexis

#endregion




using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace SuperMemoAssistant.Sys.Windows.Markup
{
  /// <summary>https://stackoverflow.com/questions/37212114/datatrigger-when-greater-than-a-number</summary>
  public class LesserThan : MarkupExtension, IValueConverter
  {
    #region Properties & Fields - Non-Public

    /// <summary>
    ///   Converter returns true if value is lesser than this. Don't let this be public,
    ///   because it's required to be initialized via the constructor.
    /// </summary>
    protected double Operand { get; set; }

    #endregion




    #region Constructors

    /// <summary>
    ///   The only public constructor is one that requires a double argument. Because of that,
    ///   the XAML editor will put a blue squiggly on it if the argument is missing in the XAML.
    /// </summary>
    /// <param name="operand">The number to compare to</param>
    public LesserThan(double operand)
    {
      Operand = operand;
    }

    #endregion




    #region Methods Impl

    /// <summary>
    ///   When the XAML is parsed, each markup extension is instantiated and the parser asks it
    ///   to provide its value. Here, the value is us.
    /// </summary>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return System.Convert.ToDouble(value) < Operand;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
