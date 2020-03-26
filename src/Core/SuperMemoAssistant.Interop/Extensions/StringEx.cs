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
// Modified On:  2020/01/26 22:31
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using SuperMemoAssistant.Sys.Security.Cryptography;

// ReSharper disable LocalizableElement

namespace SuperMemoAssistant.Extensions
{
  public static class StringEx
  {
    #region Methods
    
    public static string GetCrc32(this string content)
    {
      Crc32  crc32 = new Crc32();
      string hash  = string.Empty;

      using (MemoryStream ms = new MemoryStream(Encoding.Default.GetBytes(content)))
        foreach (byte b in crc32.ComputeHash(ms)) hash += b.ToString("x2").ToLower();

      return hash;
    }

    public static string[] SplitLines(this string str, StringSplitOptions options = StringSplitOptions.None)
    {
      return str.Split(new[] { "\r\n", "\n", "\r" }, options);
    }

    public static string TrimStart(this string str, params string[] starts)
    {
      var start = starts.FirstOrDefault(str.StartsWith);

      return start != null
        ? start == str ? string.Empty : str.Substring(start.Length)
        : str;
    }

    public static string TrimEnd(this string str, params string[] ends)
    {
      var end = ends.FirstOrDefault(str.EndsWith);

      return end != null
        ? end == str ? string.Empty : str.Substring(0, str.Length - end.Length)
        : str;
    }

    public static string Truncate(this string text, int maxLength)
    {
      if (string.IsNullOrEmpty(text))
        return text;

      return text.Length <= maxLength ? text : text.Substring(0, maxLength);
    }

    public static string Quotify(this string text, bool escapeQuotes = false)
    {
      if (escapeQuotes)
        text = text.Replace("\"", "\\\"");

      return $"\"{text}\"";
    }

    public static string Ellipsis(this string text, int length)
    {
      if (text.Length <= length)
        return text;

      int pos = text.IndexOf(" ", length, StringComparison.Ordinal);

      return pos >= 0 ? text.Substring(0, pos) + "..." : text;
    }

    public static string ReplaceFirst(this string text,
                                      string      search,
                                      string      replace)
    {
      int pos = text.IndexOf(search,
                             StringComparison.Ordinal);

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

    public static string ToBase64(this string plainText)
    {
      var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

      return Convert.ToBase64String(plainTextBytes);
    }

    public static string FromBase64(this string base64EncodedData)
    {
      var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);

      return Encoding.UTF8.GetString(base64EncodedBytes);
    }

    public static string CapitalizeFirst(this string str)
    {
      switch (str)
      {
        case null: throw new ArgumentNullException(nameof(str));
        case "":   throw new ArgumentException($"{nameof(str)} cannot be empty", nameof(str));
        default:   return str.First().ToString().ToUpper() + str.Substring(1);
      }
    }

    public static string Join<T>(string separator, IEnumerable<T> values, string defaultRet = null)
    {
      // ReSharper disable PossibleMultipleEnumeration
      if (values == null || values.Any() == false)
        return defaultRet;

      separator ??= string.Empty;

      return string.Join(separator, values);
      // ReSharper restore PossibleMultipleEnumeration
    }

    public static string HtmlEncode(this string text)
    {
      // call the normal HtmlEncode first
      char[]        chars       = WebUtility.HtmlEncode(text).ToCharArray();
      StringBuilder encodedText = new StringBuilder();

      foreach (char c in chars)
        if (c > 127) // above normal ASCII
          encodedText.Append("&#" + (int)c + ";");
        else
          encodedText.Append(c);

      return encodedText.Replace("&#65534;", "-\r\n")
                        .ToString();
    }

    public static string UrlEncode(this string text)
    {
      return WebUtility.UrlDecode(text);
    }

    public static string Before(this string str, string separator)
    {
      var idx = str.IndexOf(separator, StringComparison.Ordinal);

      return idx >= 0
        ? (idx == 0 ? string.Empty : str.Substring(0, idx))
        : null;
    }

    public static string After(this string str, string separator)
    {
      var idx = str.IndexOf(separator, StringComparison.Ordinal);

      return idx >= 0
        ? str.Substring(idx + separator.Length)
        : null;
    }

    #endregion
  }
}
