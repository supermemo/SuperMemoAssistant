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
// Created On:   2018/11/17 01:26
// Modified On:  2018/12/23 18:55
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using Anotar.Serilog;
using SuperMemoAssistant.Interop.SuperMemo.Components.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Components.Types;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Interop.SuperMemo.Elements
{
  public static class ElementClipboardBuilder
  {
    #region Constants & Statics
    
    private static readonly Rectangle CorsFull = new Rectangle(100,
                                                               100,
                                                               9780,
                                                               9600);
    private static readonly Rectangle CorsVSplitLeft = new Rectangle(100,
                                                                     100,
                                                                     4890,
                                                                     9600);
    private static readonly Rectangle CorsVSplitRight = new Rectangle(5000,
                                                                      100,
                                                                      4890,
                                                                      9600);
    private const string ElementFmt = @"Begin Element #1
Source={0}
Parent={1}
ParentTitle=
Priority={2}
Begin ElementInfo #1
Title={3}
Type={4}
Status=Memorized
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
    private const string ComponentsSkeleton = @"ComponentNo={0}
{1}";
    private const string ComponentsArticleText = @"ComponentNo=1
Begin Component #1
Type=HTML
Cors=(104,199,9699,9296)
DisplayAt=255
Hyperlink=0
Text={0}
TestElement=0
ReadOnly=0
FullHTML=1
Style=0
End Component #1";
    private const string ComponentsArticleHTMFile = @"ComponentNo=1
Begin Component #1
Type=HTML
Cors=(104,199,9699,9296)
DisplayAt=255
Hyperlink=0
HTMName=htm
HTMFile={0}
TestElement=0
ReadOnly=0
FullHTML=1
Style=0
End Component #1";
    private const string ComponentsArticlePicture = @"ComponentNo=2
Begin Component #1
Type=HTML
Cors=(200,400,5000,9200)
DisplayAt=255
Hyperlink=0
Text={0}
TestElement=0
ReadOnly=0
FullHTML=1
Style=0
End Component #1
Begin Component #2
Type=Image
Cors=(5395,402,4394,9196)
DisplayAt=255
Hyperlink=0
ImageName={1}
ImageFile={2}
Stretch=2
ClickPlay=0
TestElement=0
Transparent=0
Zoom=[0,0,0,0]
End Component #2";

    #endregion




    #region Methods

    public static string FromElementBuilder(ElementBuilder elemBuilder)
    {
      DateTime now            = DateTime.Now;
      string   collectionPath = Svc.SMA.Collection.Path;
      int      parentId       = elemBuilder.Parent?.Id ?? 1;
      string title = elemBuilder.Title ?? string.Empty; /*elemBuilder.Content.Substring(0,
                                                                        10);*/
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
        case ElementType.Unknown3:
        default:
          throw new NotImplementedException();
      }

      return string.Format(CultureInfo.InvariantCulture,
                           ElementFmt,
                           collectionPath,
                           parentId,
                           elemBuilder.Priority,
                           title,
                           type,
                           lastRepDate1,
                           elemBuilder.Reference?.ToString() ?? string.Empty,
                           GenerateComponentsStr(elemBuilder),
                           lastRepDate2,
                           lastRepTime);
    }

    private static string GenerateComponentsStr(ElementBuilder elemBuilder)
    {
      var components = new List<IComponent>();
      
      var txtContents = GetTextContents(elemBuilder);
      var imgContents = GetImageContents(elemBuilder);

      switch (elemBuilder.ContentType)
      {
        case ElementBuilder.ContentTypeEnum.Html:
        case ElementBuilder.ContentTypeEnum.RawText:
          if (imgContents.Any())
            throw new InvalidCastException("ElementBuilder ContentTypeEnum.Text contains non-text IContent");

          if (txtContents.Any() == false)
            throw new InvalidCastException("ElementBuilder ContentTypeEnum.Text does not contain any text IContent");

          BuildTextComponents(0,
                              txtContents.Count,
                              CorsFull.Left,
                              CorsFull.Top,
                              CorsFull.Right,
                              CorsFull.Bottom,
                              txtContents,
                              components);
          break;

        case ElementBuilder.ContentTypeEnum.Image:
          if (txtContents.Any())
            throw new InvalidCastException("ElementBuilder ContentTypeEnum.Image contains non-image IContent");

          if (imgContents.Any() == false)
            throw new InvalidCastException("ElementBuilder ContentTypeEnum.Image does not contain any image IContent");
          
          BuildImageComponents(0,
                               imgContents.Count,
                               CorsFull.Left,
                               CorsFull.Top,
                               CorsFull.Right,
                               CorsFull.Bottom,
                               imgContents,
                               components);
          break;

        case ElementBuilder.ContentTypeEnum.ImageAndRawText:
        case ElementBuilder.ContentTypeEnum.ImageAndHtml:
          if (txtContents.Any() == false)
            throw new InvalidCastException("ElementBuilder ContentTypeEnum.ImageAndText does not contain any text IContent");

          if (imgContents.Any() == false)
            throw new InvalidCastException("ElementBuilder ContentTypeEnum.ImageAndText does not contain any image IContent");
          
          BuildTextComponents(0,
                              txtContents.Count,
                              CorsVSplitLeft.Left,
                              CorsVSplitLeft.Top,
                              CorsVSplitLeft.Right,
                              CorsVSplitLeft.Bottom,
                              txtContents,
                              components);
          
          BuildImageComponents(components.Count,
                               imgContents.Count,
                               CorsVSplitRight.Left,
                               CorsVSplitRight.Top,
                               CorsVSplitRight.Right,
                               CorsVSplitRight.Bottom,
                               imgContents,
                               components);
          break;

        default:
          throw new NotImplementedException();
      }

      var compsText = components.Select(c => c.ToString())
                                .ToList();

      return string.Format(ComponentsSkeleton,
                           compsText.Count,
                           string.Join("\n", compsText));
    }

    private static void BuildTextComponents(int                                     startingIdx,
                                            int                                     totalCompCount,
                                            int                                     left,
                                            int                                     top,
                                            int                                     right,
                                            int                                     bottom,
                                            IEnumerable<ElementBuilder.TextContent> contents,
                                            List<IComponent>                        outComps)
    {
      int idx = 0;

      foreach (var content in contents)
      {
        GetCors(idx,
                totalCompCount,
                left,
                top,
                right,
                bottom,
                out int compLeft,
                out int compTop,
                out int compRight,
                out int compBottom);

        var comp = new ComponentHtmlBuilder(startingIdx + idx++,
                                            content.Text,
                                            compLeft,
                                            compTop,
                                            compRight,
                                            compBottom);
        outComps.Add(comp);
      }
    }

    private static void BuildImageComponents(int                                      startingIdx,
                                             int                                      totalCompCount,
                                             int                                      left,
                                             int                                      top,
                                             int                                      right,
                                             int                                      bottom,
                                             IEnumerable<ElementBuilder.ImageContent> contents,
                                             List<IComponent>                         outComps)
    {
      int idx = 0;

      foreach (var content in contents)
      {
        var img = Svc.SMA.Registry.Image[content.RegistryId];

        if (img == null) // || imgMember.Empty) // TODO: Why is Empty always true ?
        {
          LogTo.Error($"Error while building ImageComponent: IImage {content.RegistryId} is null. Skipping");
          continue;
        }

        var filePath = img.GetFilePath();

        if (File.Exists(filePath) == false)
        {
          LogTo.Error("Error while building ImageComponent: IImage {0} file {1} does not exist. Skipping",
                      img,
                      filePath);
          continue;
        }

        GetCors(idx,
                totalCompCount,
                left,
                top,
                right,
                bottom,
                out int compLeft,
                out int compTop,
                out int compRight,
                out int compBottom);

        var comp = new ComponentImageBuilder(startingIdx + idx++,
                                             img,
                                             compLeft,
                                             compTop,
                                             compRight,
                                             compBottom)
        {
          Stretch = content.StretchType
        };


        outComps.Add(comp);
      }
    }

    private static List<ElementBuilder.ImageContent> GetImageContents(ElementBuilder elemBuilder)
    {
      return elemBuilder.Contents
                        .Where(c => c.ContentType == ElementBuilder.ContentTypeEnum.Image)
                        .Cast<ElementBuilder.ImageContent>()
                        .ToList();
    }

    private static List<ElementBuilder.TextContent> GetTextContents(ElementBuilder elemBuilder)
    {
      return elemBuilder.Contents
                        .Where(c => (c.ContentType & ElementBuilder.ContentTypeEnum.Text) != ElementBuilder.ContentTypeEnum.None)
                        .Cast<ElementBuilder.TextContent>()
                        .ToList();
    }

    private static void GetCors(int     idxComp,
                                int     totalCompCount,
                                int     left,
                                int     top,
                                int     right,
                                int     bottom,
                                out int compLeft,
                                out int compTop,
                                out int compRight,
                                out int compBottom)
    {
      int totalWidth  = right - left;

      if (totalCompCount == 1)
      {
        compLeft   = left;
        compTop    = top;
        compRight  = totalWidth;
        compBottom = bottom;
        return;
      }

      int totalHeight = bottom - top;

      if (totalCompCount == 2)
      {
        int comp2Height = (int)(totalHeight / 2.0);

        compLeft   = left;
        compTop    = top + (idxComp == 0 ? 0 : comp2Height);
        compRight  = totalWidth;
        compBottom = comp2Height;
        return;
      }

      double nbRowAndCol = Math.Floor(Math.Log(totalCompCount,
                                               2));
      int compRow = (int)Math.Floor(idxComp / nbRowAndCol);
      int compCol = idxComp % (int)nbRowAndCol;

      int compWidth   = (int)(totalWidth / nbRowAndCol);
      int compHeight  = (int)(totalHeight / nbRowAndCol);

      compLeft   = left + compCol * compWidth;
      compTop    = top + compRow * compHeight;
      compRight  = compWidth;
      compBottom = compHeight;
    }

    #endregion
  }
}
