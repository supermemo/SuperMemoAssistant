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
// Created On:   2019/04/22 14:17
// Modified On:  2019/04/24 18:34
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Services.HTML.Extensions;

namespace SuperMemoAssistant.Services.HTML
{
  public static class HtmlUtils
  {
    #region Methods

    public static string HtmlInlineImage(this Image img, string attrs = null)
    {
      attrs = attrs ?? string.Empty;
      attrs = attrs.Trim();

      var imgBase64 = img.ToBase64(ImageFormat.Png);

      return
        $@"<img width=""{img.Width}"" height=""{img.Height}"" style=""background-image:url('data:image/png;base64,{imgBase64}'); background-repeat: no-repeat"" {attrs} />";
    }

    public static async Task InlineIFrames(this HtmlDocument doc, Uri baseUri, Func<string, Task<string>> asyncDownloadFunc)
    {
      var iframes = GetIFrames(doc, baseUri);
      var dlTasks = iframes.Select(t => asyncDownloadFunc(t.url));

      var dlResults = await Task.WhenAll(dlTasks);

      for (int i = 0; i < iframes.Count; i++)
      {
        var (iframeNode, url) = iframes[i];
        var html = dlResults[i];

        if (iframeNode == null || html == null)
          continue;
        
        var parentNode = iframeNode.ParentNode;

        var iframeHtmlDoc = new HtmlDocument();
        iframeHtmlDoc.LoadHtml(html);

        var iframeBody = iframeHtmlDoc.DocumentNode.SelectSingleNode("//body")?.InnerHtml ?? string.Empty;
        var inlineHtml = $@"<div>
<!-- inlined iframe: {url} -->
{iframeBody}
</div>";
        var inlineNode = HtmlNode.CreateNode(inlineHtml);

        parentNode.InsertBefore(inlineNode, iframeNode);
        parentNode.RemoveChild(iframeNode);
      }
    }

    /*

        var img = svgNode.ToImage();
        var imgHtml = $@"<div>
<!-- inlined iframe: {url} -->
{img.HtmlInlineImage()}
</div>";
     */

    private static List<(HtmlNode node, string url)> GetIFrames(HtmlDocument doc, Uri baseUri)
    {
      var ret = new List<(HtmlNode, string)>();

      foreach (var iframe in doc.DocumentNode.Descendants("iframe").Where(n => n.HasAttribute("src")))
      {
        string url = EnsureAbsoluteUri(baseUri, iframe.Attributes["src"].Value);

        ret.Add((iframe, url));
      }

      return ret;
    }

    public static void EnsureAbsoluteLinks(this HtmlDocument doc, Uri baseUri)
    {
      doc.EnsureAbsoluteLinks(baseUri, "img", "src");
      doc.EnsureAbsoluteLinks(baseUri, "a", "href");
    }

    public static void EnsureAbsoluteLinks(this HtmlDocument doc, Uri baseUri, string tag, string attr)
    {
      foreach (var n in doc.DocumentNode.Descendants(tag).Where(n => n.HasAttribute(attr)))
        n.Attributes[attr].Value = baseUri.EnsureAbsoluteUri(n.Attributes[attr].Value);
    }

    public static string EnsureAbsoluteUri(this Uri baseUri, string relativeOrAbsoluteUri)
    {
      try
      {
        if (IsAbsoluteUrl(relativeOrAbsoluteUri, out var uri))
          uri = uri.EnsureHttpsScheme();

        else
          uri = new Uri(
            baseUri,
            relativeOrAbsoluteUri
          );

        return uri?.AbsoluteUri;
      }
      catch
      {
        // Ignore - invalid link

        return relativeOrAbsoluteUri;
      }
    }

    public static Uri EnsureHttpsScheme(this Uri absoluteUri)
    {
      switch (absoluteUri.Scheme)
      {
        case "javascript":
          return null;

        default:
          return new UriBuilder(absoluteUri)
          {
            Scheme = Uri.UriSchemeHttps,
            Port   = -1,
          }.Uri;
      }
    }

    public static bool IsAbsoluteUrl(string url, out Uri absUri)
    {
      return Uri.TryCreate(url, UriKind.Absolute, out absUri);
    }

    #endregion
  }
}
