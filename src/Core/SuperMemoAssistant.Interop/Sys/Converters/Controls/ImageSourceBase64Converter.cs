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
// Modified On:  2020/01/27 13:37
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Anotar.Serilog;

namespace SuperMemoAssistant.Sys.Converters.Controls
{
  public class ImageSourceBase64Converter : IValueConverter
  {
    #region Constants & Statics

    private static readonly Regex RE_Base64 = new Regex(@"data:image/(?<type>.+?),(?<data>.+)", RegexOptions.Compiled);

    #endregion




    #region Methods Impl

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value == null)
        return null;

      if (!(value is string strData))
        return value;

      var match = RE_Base64.Match(strData);

      if (match.Success == false || string.IsNullOrWhiteSpace(match.Groups["data"].Value))
        return strData;

      try
      {
        var base64 = match.Groups["data"].Value;
        var binData = System.Convert.FromBase64String(base64);

        BitmapImage image = new BitmapImage();

        using (MemoryStream ms = new MemoryStream(binData))
        {
          ms.Position = 0;
          image.BeginInit();
          image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
          image.CacheOption = BitmapCacheOption.OnLoad;
          image.UriSource = null;
          image.StreamSource = ms;
          image.EndInit();
        }

        image.Freeze();

        return image;
      }
      catch (NotSupportedException ex)
      {
        LogTo.Warning(ex, "Faield to convert image");
      }

      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return value;
    }

    #endregion
  }
}
