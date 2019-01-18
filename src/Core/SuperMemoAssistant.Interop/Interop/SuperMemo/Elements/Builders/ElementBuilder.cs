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
// Created On:   2019/01/15 22:12
// Modified On:  2019/01/18 15:26
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Interop.SuperMemo.Content.Layout;
using SuperMemoAssistant.Interop.SuperMemo.Content.Layout.XamlLayouts;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.Services;

// ReSharper disable CollectionNeverQueried.Global

namespace SuperMemoAssistant.Interop.SuperMemo.Elements.Builders
{
  [Serializable]
  public class ElementBuilder
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

    #endregion




    #region Constructors

    public ElementBuilder(ElementType          type,
                          XamlLayout           layout,
                          params ContentBase[] contents)
    {
      Type = type;

      Layout = layout;

      if (contents != null)
        Contents.AddRange(contents);
      ContentType = Contents.Aggregate(
        ContentTypeFlag.None,
        (typeAcc,
         content) => typeAcc | content.ContentType
      );

      ShouldDisplay = true;
      Title         = null;
    }

    public ElementBuilder(ElementType          type,
                          params ContentBase[] contents)
      : this(type,
             null,
             contents) { }

    public ElementBuilder(ElementType type,
                          string      content,
                          bool        html = true)
      : this(type,
             new TextContent(html,
                             content)) { }

    public ElementBuilder(ElementType type,
                          string      content,
                          bool        html,
                          Encoding    encoding)
      : this(type,
             new TextContent(html,
                             content,
                             encoding)) { }

    #endregion




    #region Properties & Fields - Public

    public ElementType       Type           { get; }
    public List<ContentBase> Contents       { get; } = new List<ContentBase>();
    public ContentTypeFlag   ContentType    { get; }
    public XamlLayout        Layout         { get; private set; }
    public string            Title          { get; private set; }
    public References        Reference      { get; private set; }
    public bool              ShouldDisplay  { get; private set; }
    public double            Priority       { get; private set; }
    public IElement          Parent         { get; private set; }
    public IConcept          Concept        { get; private set; }
    public List<IConcept>    LinkedConcepts { get; } = new List<IConcept>();

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      DateTime now            = DateTime.Now;
      string   collectionPath = Svc.SMA.Collection.Path;
      int      parentId       = Parent?.Id ?? 1;
      string   title          = Title ?? string.Empty; /*Content.Substring(0, 10);*/
      string lastRepDate1 = DateTime.Today.ToString("dd.MM.yy",
                                                    CultureInfo.InvariantCulture);
      string lastRepDate2 = DateTime.Today.ToString("dd.MM.yyyy",
                                                    CultureInfo.InvariantCulture);
      double lastRepTime = now.Hour + (now.Minute * 60 + now.Second) / 3600.0;
      string type;

      switch (Type)
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

      if (Layout == null)
        Layout = LayoutManager.Generic;

      return string.Format(CultureInfo.InvariantCulture,
                           ElementFmt,
                           collectionPath,
                           parentId,
                           Priority,
                           title,
                           type,
                           lastRepDate1,
                           Reference?.ToString() ?? string.Empty,
                           Layout.Build(Contents),
                           lastRepDate2,
                           lastRepTime);
    }

    #endregion




    #region Methods

    public ElementBuilder WithTitle(string title)
    {
      Title = title;
      return this;
    }

    public ElementBuilder WithReference(Func<References, References> refBuilder)
    {
      Reference = refBuilder(new References());
      return this;
    }

    public ElementBuilder Display()
    {
      ShouldDisplay = true;
      return this;
    }

    public ElementBuilder DoNotDisplay()
    {
      ShouldDisplay = false;
      return this;
    }

    public ElementBuilder WithPriority(double priority)
    {
      Priority = priority;
      return this;
    }

    public ElementBuilder WithParent(IElement parent)
    {
      Parent = parent;
      return this;
    }

    public ElementBuilder WithConcept(IConcept concept)
    {
      Concept = concept;
      return this;
    }

    public ElementBuilder AddLinkedConcepts(IEnumerable<IConcept> concepts)
    {
      LinkedConcepts.AddRange(concepts);
      return this;
    }

    public ElementBuilder AddLinkedConcept(IConcept concept)
    {
      LinkedConcepts.Add(concept);
      return this;
    }

    #endregion
  }
}
