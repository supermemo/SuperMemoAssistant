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
// Created On:   2019/04/22 14:09
// Modified On:  2019/04/22 14:14
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using Anotar.Serilog;
using HtmlAgilityPack;
using SuperMemoAssistant.Services.HTML.Models;

namespace SuperMemoAssistant.Services.HTML.Extensions
{
  public static class HtmlFilterEx
  {
    #region Methods

    public static string Filter(this HtmlFilter filter, string content)
    {
      switch (filter.Type)
      {
        case HtmlFilterType.Regex:
          var filteredContents = new List<string>();
          var regex            = new Regex(filter.Filter, RegexOptions.Singleline);
          var matches          = regex.Matches(content);

          for (int i = 0; i < matches.Count; i++)
          {
            var match = matches[i];

            if (match.Success == false)
              continue;

            for (int j = 1; j < match.Groups.Count; j++)
              filteredContents.Add(match.Groups[j].Value);
          }

          if (filteredContents.Any() == false)
            return null;

          content = string.Join("\r\n", filteredContents);
          break;

        case HtmlFilterType.XPath:
          try
          {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            filteredContents = htmlDoc.DocumentNode
                                      .SelectNodes(filter.Filter)
                                      ?.Select(n => n.OuterHtml)
                                      .ToList();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (filteredContents == null || filteredContents.Any() == false)
              return null;

            content = string.Join("\r\n", filteredContents);
          }
          catch (Exception ex)
          {
            LogTo.Error(ex, $"Invalid XPath filter '{filter}'");
            throw;
          }
          break;

        default:
          throw new NotImplementedException($"Feed filter type {filter.Type} not implemented");
      }

      if (filter.Children.Any())
      {
        var subContents = filter.Children.Select(f => f.Filter(content)).ToList();

        if (subContents.Any() == false)
          return null;

        return string.Join("\r\n", subContents);
      }

      return content;
    }

    public static bool ValidateFilter(this HtmlFilter filter, out string error)
    {
      error = null;

      switch (filter.Type)
      {
        case HtmlFilterType.Regex:
          try
          {
            // ReSharper disable once ObjectCreationAsStatement
            new Regex(filter.Filter);

            return true;
          }
          catch (Exception ex)
          {
            error = ex.Message;
          }

          return false;

        case HtmlFilterType.XPath:
          try
          {
            XPathExpression.Compile(filter.Filter);

            return true;
          }
          catch (Exception ex)
          {
            error = ex.Message;
          }

          return false;

        default:
          throw new NotImplementedException("Invalid filter type");
      }
    }

    #endregion
  }
}
