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
// Created On:   2019/01/15 22:14
// Modified On:  2019/01/15 22:14
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using SuperMemoAssistant.Extensions;

namespace SuperMemoAssistant.Interop.SuperMemo.Elements.Builders
{
  [Serializable]
  public class References
  {
    #region Properties & Fields - Public

    public string                    Author  { get; set; }
    public string                    Title   { get; set; }
    public List<(string, DateTime?)> Dates   { get; } = new List<(string, DateTime?)>();
    public string                    Source  { get; set; }
    public string                    Link    { get; set; }
    public string                    Email   { get; set; }
    public string                    Comment { get; set; }

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      List<string> refParts = new List<string>();

      if (string.IsNullOrWhiteSpace(Title) == false)
        refParts.Add($"#Title: {Title.HtmlEncode()}");

      if (string.IsNullOrWhiteSpace(Author) == false)
        refParts.Add(
          string.IsNullOrWhiteSpace(Email)
            ? $"#Author: {Author.HtmlEncode()}"
            : $"#Author: {Author.HtmlEncode()} [mailto:{Email.HtmlEncode()}]"
        );

      if (Dates != null && Dates.Any())
      {
        var dateStrs = Dates.Select(d => string.Format(d.Item1,
                                                       d.Item2?.ToString("MMM dd, yyyy, hh:mm:ss")));
        var dateStr = string.Join(" ; ",
                                  dateStrs);

        refParts.Add($"#Date: {dateStr.HtmlEncode()}");
      }

      if (string.IsNullOrWhiteSpace(Source) == false)
        refParts.Add($"#Source: {Source.HtmlEncode()}");

      if (string.IsNullOrWhiteSpace(Link) == false)
        refParts.Add($"#Link: <a href=\"{Link.UrlEncode()}\">{Link.HtmlEncode()}</a>");

      if (string.IsNullOrWhiteSpace(Email) == false)
        refParts.Add($"#E-mail: {Email.HtmlEncode()}");

      if (string.IsNullOrWhiteSpace(Comment) == false)
        refParts.Add($"#Comment: {Comment.HtmlEncode()}");

      if (refParts.Any() == false)
        return string.Empty;

      return string.Format(SMConst.Elements.ReferenceFormat,
                           string.Join("<br>",
                                       refParts));
    }

    #endregion




    #region Methods

    public References WithAuthor(string author)
    {
      Author = author;
      return this;
    }

    public References WithTitle(string title)
    {
      Title = title;
      return this;
    }

    public References WithDate(DateTime date,
                               string   fmt = "{0}")
    {
      Dates.Clear();

      return AddDate(date,
                     fmt);
    }

    public References WithDate(string date)
    {
      Dates.Clear();

      return AddDate(date);
    }

    public References AddDate(DateTime date,
                              string   fmt = "{0}")
    {
      Dates.Add((fmt, date));

      return this;
    }

    public References AddDate(string date)
    {
      if (date != null)
        Dates.Add((date, null));

      return this;
    }

    public References WithSource(string source)
    {
      Source = source;
      return this;
    }

    public References WithLink(string link)
    {
      Link = link;
      return this;
    }

    public References WithEmail(string email)
    {
      Email = email;
      return this;
    }

    public References WithComment(string comment)
    {
      Comment = comment;
      return this;
    }

    #endregion
  }
}
