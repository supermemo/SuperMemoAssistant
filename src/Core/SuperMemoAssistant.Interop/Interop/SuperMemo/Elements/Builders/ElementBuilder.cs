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
// Modified On:  2020/01/28 18:02
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

// ReSharper disable CollectionNeverQueried.Global

namespace SuperMemoAssistant.Interop.SuperMemo.Elements.Builders
{
  [Serializable]
  public class ElementBuilder
  {
    #region Constructors

    public ElementBuilder(ElementType          type,
                          string               layoutName = null,
                          params ContentBase[] contents)
    {
      Type = type;

      Layout = layoutName;

      if (contents != null)
        Contents.AddRange(contents);

      ContentType = Contents.Aggregate(
        ContentTypeFlag.None,
        (typeAcc, content) => typeAcc | content.ContentType
      );

      Status        = ElementStatus.Memorized;
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

    public ElementType       Type               { get; }
    public List<ContentBase> Contents           { get; } = new List<ContentBase>();
    public ContentTypeFlag   ContentType        { get; }
    public string            Layout             { get; private set; }
    public string            Title              { get; private set; }
    public References        Reference          { get; private set; }
    public bool              ShouldDisplay      { get; private set; }
    public double            Priority           { get; private set; }
    public IElement          Parent             { get; private set; }
    public IConcept          Concept            { get; private set; }
    public ElementStatus     Status             { get; private set; }
    public bool              ForceGenerateTitle { get; private set; }
    public List<IConcept>    LinkedConcepts     { get; } = new List<IConcept>();

    #endregion




    #region Methods

    public ElementBuilder WithTitle(string title)
    {
      Title = title;
      return this;
    }

    public ElementBuilder WithLayout(string layoutName)
    {
      Layout = layoutName;
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

    public ElementBuilder WithStatus(ElementStatus status)
    {
      if (status == ElementStatus.Deleted)
        throw new ArgumentException("New element can't be deleted");

      Status = status;
      return this;
    }

    public ElementBuilder WithForcedGeneratedTitle()
    {
      ForceGenerateTitle = true;
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
