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
// Modified On:  2020/03/05 22:23
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;

namespace SuperMemoAssistant.Sys.Windows.Data
{
  /// <summary>
  /// Converts a pure base64 string, or a "data:image/(type),(base64)" formatted string to a <see cref="BitmapImage"/>
  /// </summary>
  public class ImageSourceBase64Converter : IValueConverter
  {
    #region Constants & Statics

    private static readonly Regex RE_Base64 = new Regex(@"data:image/(?<type>.+?),(?<data>.+)", RegexOptions.Compiled);

    #endregion




    #region Methods Impl

    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value == null)
        return null;

      if (!(value is string strData))
        return value;

      if (string.IsNullOrWhiteSpace(strData))
        return null;

      var match = RE_Base64.Match(strData);
      var base64 = match.Success == false || string.IsNullOrWhiteSpace(match.Groups["data"].Value)
        ? strData
        : match.Groups["data"].Value;

      try
      {
        byte[] binData = System.Convert.FromBase64String(base64);

        BitmapImage image = new BitmapImage();

        using (MemoryStream ms = new MemoryStream(binData))
        {
          ms.Position = 0;
          image.BeginInit();
          image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
          image.CacheOption   = BitmapCacheOption.OnLoad;
          image.UriSource     = null;
          image.StreamSource  = ms;
          image.EndInit();
        }

        image.Freeze();

        return image;
      }
      catch (FormatException ex)
      {
        LogTo.Warning(ex, $"the provided value is not in a valid base64 format: '{base64.Truncate(30)}'");
      }
      catch (NotSupportedException ex)
      {
        LogTo.Warning(ex, $"Failed to convert image from base64: '{base64.Truncate(30)}'");
      }

      return null;
    }
    
    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return value;
    }

    #endregion
  }
}
