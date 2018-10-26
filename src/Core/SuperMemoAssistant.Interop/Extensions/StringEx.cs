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
// Created On:   2018/06/03 03:18
// Modified On:  2018/10/26 21:45
// Modified By:  Alexis

#endregion




using System;

namespace SuperMemoAssistant.Extensions
{
  public static class StringEx
  {
    #region Methods

    public static string ReplaceFirst(this string text,
                                      string      search,
                                      string      replace)
    {
      int pos = text.IndexOf(search);

      return pos < 0
        ? text
        : text.Substring(0,
                         pos) + replace + text.Substring(pos + search.Length);
    }

    public static string ReplaceNth(this string text,
                                    string      search,
                                    string      replace,
                                    int         nth)
    {
      int pos = text.NthIdexOf(search,
                               nth);

      return pos < 0
        ? text
        : text.Substring(0,
                         pos) + replace + text.Substring(pos + search.Length);
    }

    public static int NthIdexOf(this string text,
                                string      search,
                                int         nth)
    {
      int pos = -1;

      while (nth > 0 && (pos = text.IndexOf(search,
                                            pos + 1,
                                            StringComparison.Ordinal)) >= 0)
        nth--;

      return pos;
    }

    public static string Base64Encode(this string plainText)
    {
      var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

      return Convert.ToBase64String(plainTextBytes);
    }

    public static string Base64Decode(this string base64EncodedData)
    {
      var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);

      return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

    #endregion
  }
}
