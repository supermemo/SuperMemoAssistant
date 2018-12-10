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
// Modified On:  2018/12/04 22:29
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Interop.SuperMemo.Elements
{
  public static class ElementClipboardBuilder
  {
    #region Constants & Statics

    private const string ElementClipboardFormat = @"Begin Element #1
Source={0}
Parent={1}
ParentTitle=
Priority=0
Begin ElementInfo #1
Title={2}
Type={3}
Status=Memorized
FirstGrade=8
Ordinal=10004.000000
Repetitions=1
Lapses=0
Interval=1
LastRepetition={4}
AFactor=1.200
UFactor=1.000
ForgettingIndex=10
Reference={5}
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
{6}
Begin RepHist #1
ElNo=1 Rep=1 Date={4} Hour={7:#.###} Int=0 Grade=10 Laps=0 Priority=0
End RepHist #1
End Element #1";
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
      string lastRep     = DateTime.Today.ToString("dd.MM.yy");
      double lastRepTime = Math.Floor((now.Minute * 60 + now.Second) * 1000 / 3600.0);
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

      return string.Format(ElementClipboardFormat,
                           collectionPath,
                           parentId,
                           title,
                           type,
                           lastRep,
                           elemBuilder.Reference?.ToString() ?? string.Empty,
                           GenerateComponentsStr(elemBuilder),
                           lastRepTime);
    }

    private static string GenerateComponentsStr(ElementBuilder elemBuilder)
    {
      switch (elemBuilder.ContentType)
      {
        case ElementBuilder.ContentTypeEnum.Html:
        case ElementBuilder.ContentTypeEnum.RawText:
          var txtContent = (ElementBuilder.TextContent)elemBuilder.Contents[0];

          if (txtContent == null)
            throw new InvalidCastException("ContentTypeEnum.Html or ContentTypeEnum.RawText contained a non-text IContent");

          return string.Format(
            ComponentsArticleHTMFile,
            WriteToFile(txtContent)
          );

        case ElementBuilder.ContentTypeEnum.Image:
          var imgContent = (ElementBuilder.ImageContent)elemBuilder.Contents[0];

          if (imgContent == null)
            throw new InvalidCastException("ContentTypeEnum.Image contained a non-image IContent");

          var imgMember = Svc.SMA.Registry.Image[imgContent.RegistryId];

          if (imgMember == null) // || imgMember.Empty) // TODO: Why is Empty always true ?
            throw new ArgumentException($"Image member {imgContent.RegistryId} doesn't exist or is deleted.");

          var filePath = imgMember.GetFilePath();

          if (File.Exists(filePath) == false)
            throw new InvalidOperationException($"File path '{filePath}' doesn't exist for image member id {imgContent.RegistryId}");

          return string.Format(
            ComponentsArticlePicture,
            imgMember.Name,
            imgMember.Name,
            filePath
          );

        case ElementBuilder.ContentTypeEnum.ImageAndRawText:
        case ElementBuilder.ContentTypeEnum.ImageAndHtml:
          throw new NotImplementedException();

        default:
          throw new NotImplementedException();
      }
    }

    private static string WriteToFile(ElementBuilder.TextContent textContent)
    {
      var filePath = Path.Combine(Path.GetTempPath(),
                                  "sm_element.htm");

      File.WriteAllText(filePath,
                        textContent.Text + "\r\n<span />");

      return filePath;
    }

    #endregion
  }
}
