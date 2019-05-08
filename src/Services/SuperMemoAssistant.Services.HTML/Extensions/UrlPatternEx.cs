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
// Created On:   2019/04/22 15:19
// Modified On:  2019/04/22 15:41
// Modified By:  Alexis

#endregion




using System;
using System.Text.RegularExpressions;
using Anotar.Serilog;
using SuperMemoAssistant.Services.HTML.Models;

namespace SuperMemoAssistant.Services.HTML.Extensions
{
  public static class UrlPatternEx
  {
    #region Methods

    public static bool Match(this UrlPattern urlPattern, string url)
    {
      switch (urlPattern.Type)
      {
        case UrlPatternType.Hostname:
          try
          {
            return new Uri(url).Host == urlPattern.Pattern;
          }
          catch
          {
            return false;
          }

        case UrlPatternType.StartWith:
          return url.StartsWith(urlPattern.Pattern);

        case UrlPatternType.Contains:
          return url.Contains(urlPattern.Pattern);

        case UrlPatternType.EndWith:
          return url.EndsWith(urlPattern.Pattern);

        case UrlPatternType.Regex:
          try
          {
            return urlPattern.Regex.Match(url).Success;
          }
          catch (Exception ex)
          {
            LogTo.Error(ex, $"Exception while matching regex url pattern '{urlPattern.Pattern}' with '{url}'.");
            throw;
          }

        default:
          throw new NotImplementedException($"Feed filter type {urlPattern.Type} not implemented");
      }
    }

    public static bool ValidateFilter(this UrlPattern urlPattern, out string error)
    {
      error = null;

      switch (urlPattern.Type)
      {
        case UrlPatternType.Regex:
          try
          {
            // ReSharper disable once ObjectCreationAsStatement
            new Regex(urlPattern.Pattern);

            return true;
          }
          catch (Exception ex)
          {
            error = ex.Message;
          }

          return false;

        default:
          return true;
      }
    }

    #endregion
  }
}
