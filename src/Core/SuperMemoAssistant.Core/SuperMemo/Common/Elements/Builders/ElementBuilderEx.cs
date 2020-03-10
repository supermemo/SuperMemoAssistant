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
// Modified On:  2020/01/28 18:50
// Modified By:  Alexis

#endregion




using System;
using System.Globalization;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SuperMemo.Common.Content.Layout;
using SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlLayouts;

namespace SuperMemoAssistant.SuperMemo.Common.Elements.Builders
{
  public static class ElementBuilderEx
  {
    #region Constants & Statics

    private const string ElementFmt = @"Begin Element #1
Source={0}
Parent={1}
ParentTitle=
Priority={2}
Begin ElementInfo #1
Title={3}
Type={4}
Status={8}
FirstGrade=8
Ordinal=10004.000000
Repetitions=1
Lapses=0
Interval=1
LastRepetition={5}
AFactor=1.200
UFactor=1.000
ForgettingIndex=10
Reference={6}
SourceArticle=0
End ElementInfo #1
ElementColor=-16777211
AutoPlay=1
BackgroundImage=
BackgroundFile=
BackgroundStyle=Tile
Scaled=1
ReadPointComponent=0
ReadPointStart=0
ReadPointLength=0
ReadPointScrollTop=0
{7}
End Element #1";

    #endregion




    #region Methods

    public static string ToElementString(this ElementBuilder elemBuilder)
    {
      string   collectionPath = Core.SM.Collection.Path;
      int      parentId       = elemBuilder.Parent?.Id ?? 1;
      string   lastRepDate1   = DateTime.Today.ToString("dd.MM.yy", CultureInfo.InvariantCulture);
      string   type;

      switch (elemBuilder.Type)
      {
        case ElementType.Topic:
          type = "Topic";
          break;

        case ElementType.Item:
          type = "Item";
          break;

        case ElementType.ConceptGroup:
        case ElementType.Task:
        case ElementType.Template:
        default:
          throw new NotImplementedException();
      }

      string title = (elemBuilder.GuessTitle() ?? string.Empty).Replace("\n", "").Replace("\r", "");
      XamlLayout layout = LayoutManager.Instance.GetLayout(elemBuilder.Layout)
        ?? LayoutManager.DefaultOrGenericLayout;

      var ret = string.Format(CultureInfo.InvariantCulture,
                              ElementFmt,
                              collectionPath,
                              parentId,
                              elemBuilder.Priority,
                              title,
                              type,
                              lastRepDate1,
                              elemBuilder.Reference?.ToString() ?? string.Empty,
                              layout.Build(elemBuilder.Contents),
                              elemBuilder.Status);


      ret = EncodeForSupermemo(ret);

      return ret;
    }

    private static string EncodeForSupermemo(string elemDesc)
    {
      StringBuilder encodedElemDesc = new StringBuilder();

      foreach (var c in elemDesc.ToCharArray())
      {
        if (c <= 127)
        {
          encodedElemDesc.Append(c);
          continue;
        }

        var utfBytes = Encoding.UTF8.GetBytes(new[] { c });

        foreach (var utfByte in utfBytes)
          encodedElemDesc.Append(Encoding.Default.GetChars(new[] { utfByte }));
      }

      return encodedElemDesc.ToString();
    }

    private static string GuessTitle(this ElementBuilder elemBuilder)
    {
      var title = elemBuilder.Title ?? elemBuilder.Reference?.Title ?? string.Empty;

      if (!elemBuilder.ForceGenerateTitle && !string.IsNullOrWhiteSpace(title))
        return title;

      var txtContent = (TextContent)elemBuilder
                                    .Contents
                                    .FirstOrDefault(c => (c.ContentType & ContentTypeFlag.Text) != ContentTypeFlag.None);

      if (txtContent != null)
      {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(txtContent.Text);

        var text = doc.DocumentNode
                      .SelectNodes("//text()")
                      ?.Select(n => n.InnerText);

        title = text != null ? string.Join(" ", text) : txtContent.Text;
        title = HtmlEntity.DeEntitize(title);

        return title.Substring(0, Math.Min(title.Length, 80));
      }

      return title;
    }

    #endregion
  }
}
