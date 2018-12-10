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
// Modified On:  2018/11/19 16:16
// Modified By:  Alexis

#endregion




using System;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Interop.SuperMemo.Elements
{
  public static class ElementClipboard
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
Reference=
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
ComponentNo=1
Begin Component #1
Type=HTML
Cors=(104,199,9699,9296)
DisplayAt=255
Hyperlink=0
Text=
TestElement=0
ReadOnly=0
FullHTML=1
Style=0
End Component #1
Begin RepHist #1
ElNo=1 Rep=1 Date={4} Hour={5:#.###} Int=0 Grade=10 Laps=0 Priority=0
End RepHist #1
End Element #1";

    #endregion




    #region Methods

    public static string FromElementBuilder(ElementBuilder elemBuilder)
    {
      DateTime now            = DateTime.Now;
      string   collectionPath = Svc.SMA.Collection.Path;
      int      parentId       = elemBuilder.Parent?.Id ?? 1;
      string title = elemBuilder.Title ?? string.Empty; /*elemBuilder.Content.Substring(0,
                                                                        10);*/
      string type = elemBuilder.Type == Models.ElementType.Topic
        ? "Topic"
        : "Item";
      string lastRep     = DateTime.Today.ToString("dd.MM.yy");
      double lastRepTime = Math.Floor((now.Minute * 60 + now.Second) * 1000 / 3600.0);

      return string.Format(ElementClipboardFormat,
                           collectionPath,
                           parentId,
                           title,
                           type,
                           lastRep,
                           lastRepTime);
    }

    #endregion
  }
}
