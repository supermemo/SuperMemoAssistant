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
// Created On:   2018/07/27 12:55
// Modified On:  2018/11/23 19:47
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Drawing;
using SuperMemoAssistant.Interop.SuperMemo.Components;
using SuperMemoAssistant.Interop.SuperMemo.Components.Types;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.Sys.Drawing;

namespace SuperMemoAssistant.Interop.SuperMemo.Elements
{
  [Serializable]
  public class ElementBuilder
  {
    #region Properties & Fields - Non-Public

    private List<IComponent> ComponentsInternal     { get; set; }
    private List<IConcept>   LinkedConceptsInternal { get; set; }

    #endregion




    #region Constructors

    public ElementBuilder(ElementType type,
                          string      content,
                          bool        html = true)
    {
      Type    = type;
      Content = content;
      ContentType = html
        ? ContentType = ContentTypeEnum.Html
        : ContentTypeEnum.RawText;

      ShouldDisplay = true;
      Title         = null;

      LinkedConceptsInternal = new List<IConcept>();
      ComponentsInternal     = new List<IComponent>();
    }

    public ElementBuilder(ElementType type,
                          Image       content)
    {
      Type        = type;
      Content     = new ImageWrapper(content);
      ContentType = ContentTypeEnum.Image;

      ShouldDisplay = true;
      Title         = null;

      LinkedConceptsInternal = new List<IConcept>();
      ComponentsInternal     = new List<IComponent>();
    }

    #endregion




    #region Properties & Fields - Public

    public ElementType             Type           { get; }
    public object                  Content        { get; }
    public ContentTypeEnum         ContentType    { get; }
    public string                  Title          { get; private set; }
    public bool                    ShouldDisplay  { get; private set; }
    public int                     Id             { get; private set; }
    public IElement                Parent         { get; private set; }
    public IConcept                Concept        { get; private set; }
    public IEnumerable<IConcept>   LinkedConcepts => LinkedConceptsInternal;
    public IEnumerable<IComponent> Components     => ComponentsInternal;

    #endregion




    #region Methods

    public ElementBuilder WithId(int id)
    {
      throw new NotImplementedException();

      Id = id;
      return this;
    }

    public ElementBuilder WithTitle(string title)
    {
      Title = title;
      return this;
    }

    public ElementBuilder Display()
    {
      ShouldDisplay = true;
      return this;
    }

    public ElementBuilder DoNotDisplay()
    {
      throw new NotImplementedException(); // TODO: Find a SM method to hook for that purpose

      ShouldDisplay = false;
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
      throw new NotImplementedException();

      LinkedConceptsInternal.AddRange(concepts);
      return this;
    }

    public ElementBuilder AddLinkedConcept(IConcept concept)
    {
      throw new NotImplementedException();

      LinkedConceptsInternal.Add(concept);
      return this;
    }

    public ElementBuilder AddComponentGroup(IComponentGroup componentGroup)
    {
      throw new NotImplementedException();

      ComponentsInternal.AddRange(componentGroup.Components);
      return this;
    }

    public ElementBuilder AddComponents(IEnumerable<IComponent> components)
    {
      throw new NotImplementedException();

      ComponentsInternal.AddRange(components);
      return this;
    }

    public ElementBuilder AddComponent(IComponent component)
    {
      throw new NotImplementedException();

      ComponentsInternal.Add(component);
      return this;
    }

    #endregion




    #region Enums

    public enum ContentTypeEnum
    {
      RawText,
      Html,
      Image,
    }

    #endregion
  }
}
