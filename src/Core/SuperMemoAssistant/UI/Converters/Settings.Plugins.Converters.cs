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

#endregion




namespace SuperMemoAssistant.UI.Converters
{
  using System;
  using System.Globalization;
  using FontAwesome5;
  using PluginManager.Models;
  using Sys.Windows.Data;

  public class StartPauseIconPluginStatusConverter : OneWayValueConverter
  {
    #region Methods Impl

    /// <inheritdoc />
    public override object Convert(object      value,
                                   Type        targetType,
                                   object      parameter,
                                   CultureInfo culture)
    {
      bool isDev = false;

      if (!(value is PluginStatus pluginStatus))
        throw new ArgumentException($"{nameof(value)} must be of type {nameof(PluginStatus)}");

      if (parameter is string isDevStr)
        _ = bool.TryParse(isDevStr, out isDev);

      switch (pluginStatus)
      {
        case PluginStatus.Starting:
        case PluginStatus.Connected:
        case PluginStatus.Stopping:
          return EFontAwesomeIcon.Solid_Pause;

        default:
          return isDev
            ? EFontAwesomeIcon.Solid_Bug
            : EFontAwesomeIcon.Solid_Play;
      }
    }

    #endregion
  }
}
