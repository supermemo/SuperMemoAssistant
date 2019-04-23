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
// Created On:   2019/03/02 18:29
// Modified On:  2019/04/15 00:09
// Modified By:  Alexis

#endregion




using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Content.Layout;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Content.Layout.XamlLayouts;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Builders
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
Status={10}
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
Begin RepHist #1
ElNo=1 Rep=1 Date={8} Hour={9:0.000} Int=0 Grade=10 Laps=0 Priority={2}
End RepHist #1
End Element #1";

    #endregion




    #region Methods

    public static string ToElementString(this ElementBuilder elemBuilder)
    {
      DateTime now            = DateTime.Now;
      string   collectionPath = Svc.SMA.Collection.Path;
      int      parentId       = elemBuilder.Parent?.Id ?? 1;
      string lastRepDate1 = DateTime.Today.ToString("dd.MM.yy",
                                                    CultureInfo.InvariantCulture);
      string lastRepDate2 = DateTime.Today.ToString("dd.MM.yyyy",
                                                    CultureInfo.InvariantCulture);
      double lastRepTime = now.Hour + (now.Minute * 60 + now.Second) / 3600.0;
      string type;

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
                              lastRepDate2,
                              lastRepTime,
                              elemBuilder.Status);


      return ret;
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

        title = string.Join(" ", doc.DocumentNode.SelectNodes("//text()").Select(n => n.InnerText));
        title = HtmlEntity.DeEntitize(title);

        return title.Substring(0, Math.Min(title.Length, 80));
      }

      return title;
    }

    #endregion
  }
}
