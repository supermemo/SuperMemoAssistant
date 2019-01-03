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
// Modified On:  2018/12/29 23:22
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperMemoAssistant.Interop.SuperMemo.Components;
using SuperMemoAssistant.Interop.SuperMemo.Components.Models;
using SuperMemoAssistant.Interop.SuperMemo.Components.Types;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Elements
{
  [Serializable]
  public class ElementBuilder
  {
    #region Properties & Fields - Non-Public

    // ReSharper disable once CollectionNeverUpdated.Local
    private List<IComponent> ComponentsInternal { get; }
    // ReSharper disable once CollectionNeverUpdated.Local
    private List<IConcept> LinkedConceptsInternal { get; }

    #endregion




    #region Constructors

    public ElementBuilder(ElementType       type,
                          params IContent[] contents)
    {
      Type = type;

      Contents.AddRange(contents);
      ContentType = Contents.Aggregate(
        ContentTypeEnum.None,
        (typeAcc,
         content) => typeAcc | content.ContentType
      );

      ShouldDisplay = true;
      Title         = null;

      LinkedConceptsInternal = new List<IConcept>();
      ComponentsInternal     = new List<IComponent>();
    }

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

    public ElementType             Type           { get; }
    public List<IContent>          Contents       { get; } = new List<IContent>();
    public ContentTypeEnum         ContentType    { get; }
    public string                  Title          { get; private set; }
    public ElemReference           Reference      { get; private set; }
    public bool                    ShouldDisplay  { get; private set; }
    public int                     Id             { get; private set; } = -1;
    public double                  Priority       { get; private set; } = 0;
    public IElement                Parent         { get; private set; }
    public IConcept                Concept        { get; private set; }
    public IEnumerable<IConcept>   LinkedConcepts => LinkedConceptsInternal;
    public IEnumerable<IComponent> Components     => ComponentsInternal;

    #endregion




    #region Methods

    public ElementBuilder WithId(int id)
    {
      throw new NotImplementedException();

      //Id = id;
      //return this;
    }

    public ElementBuilder WithTitle(string title)
    {
      Title = title;
      return this;
    }

    public ElementBuilder WithReference(Func<ElemReference, ElemReference> refBuilder)
    {
      Reference = refBuilder(new ElemReference());
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
      throw new NotImplementedException();

      //LinkedConceptsInternal.AddRange(concepts);
      //return this;
    }

    public ElementBuilder AddLinkedConcept(IConcept concept)
    {
      throw new NotImplementedException();

      //LinkedConceptsInternal.Add(concept);
      //return this;
    }

    public ElementBuilder AddComponentGroup(IComponentGroup componentGroup)
    {
      throw new NotImplementedException();

      //ComponentsInternal.AddRange(componentGroup.Components);
      //return this;
    }

    public ElementBuilder AddComponents(IEnumerable<IComponent> components)
    {
      throw new NotImplementedException();

      //ComponentsInternal.AddRange(components);
      //return this;
    }

    public ElementBuilder AddComponent(IComponent component)
    {
      throw new NotImplementedException();

      //ComponentsInternal.Add(component);
      //return this;
    }

    #endregion




    #region Enums

    [Flags]
    public enum ContentTypeEnum
    {
      None            = 0,
      RawText         = 1,
      Html            = 2,
      Image           = 4,
      Text            = RawText | Html,
      ImageAndRawText = RawText | Image,
      ImageAndHtml    = Html | Image,
      ImageAndText    = RawText | Html | Image,
    }

    #endregion




    [Serializable]
    public class ElemReference
    {
      #region Properties & Fields - Public

      public string                    Author  { get; set; }
      public string                    Title   { get; set; }
      public List<(string, DateTime?)> Dates   { get; } = new List<(string, DateTime?)>();
      public string                    Source  { get; set; }
      public string                    Link    { get; set; }
      public string                    Email   { get; set; }
      public string                    Comment { get; set; }

      #endregion




      #region Methods Impl

      public override string ToString()
      {
        List<string> refParts = new List<string>();

        if (string.IsNullOrWhiteSpace(Title) == false)
          refParts.Add($"#Title: {Title}");

        if (string.IsNullOrWhiteSpace(Author) == false)
          refParts.Add(
            string.IsNullOrWhiteSpace(Email)
              ? $"#Author: {Author}"
              : $"#Author: {Author} [mailto:{Email}]"
          );

        if (Dates != null && Dates.Any())
        {
          var dateStrs = Dates.Select(d => string.Format(d.Item1,
                                                         d.Item2?.ToString("MMM dd, yyyy, hh:mm:ss")));
          var dateStr = string.Join(" ; ",
                                    dateStrs);

          refParts.Add($"#Date: {dateStr}");
        }

        if (string.IsNullOrWhiteSpace(Source) == false)
          refParts.Add($"#Source: {Source}");

        if (string.IsNullOrWhiteSpace(Link) == false)
          refParts.Add($"#Link: <a href=\"{Link}\">{Link}</a>");

        if (string.IsNullOrWhiteSpace(Email) == false)
          refParts.Add($"#E-mail: {Email}");

        if (string.IsNullOrWhiteSpace(Comment) == false)
          refParts.Add($"#Comment: {Comment}");

        if (refParts.Any() == false)
          return string.Empty;

        return string.Format(SMConst.Elements.ReferenceFormat,
                             string.Join("<br>",
                                         refParts));
      }

      #endregion




      #region Methods

      public ElemReference WithAuthor(string author)
      {
        Author = author;
        return this;
      }

      public ElemReference WithTitle(string title)
      {
        Title = title;
        return this;
      }

      public ElemReference WithDate(DateTime date,
                                    string   fmt = "{0}")
      {
        Dates.Clear();

        return AddDate(date,
                       fmt);
      }

      public ElemReference WithDate(string date)
      {
        Dates.Clear();

        return AddDate(date);
      }

      public ElemReference AddDate(DateTime date,
                                   string   fmt = "{0}")
      {
        Dates.Add((fmt, date));

        return this;
      }

      public ElemReference AddDate(string date)
      {
        if (date != null)
          Dates.Add((date, null));

        return this;
      }

      public ElemReference WithSource(string source)
      {
        Source = source;
        return this;
      }

      public ElemReference WithLink(string link)
      {
        Link = link;
        return this;
      }

      public ElemReference WithEmail(string email)
      {
        Email = email;
        return this;
      }

      public ElemReference WithComment(string comment)
      {
        Comment = comment;
        return this;
      }

      #endregion
    }

    public interface IContent
    {
      ContentTypeEnum ContentType { get; }
    }

    [Serializable]
    public class TextContent : IContent
    {
      #region Constructors

      public TextContent(bool   html,
                         string text)
        : this(html,
               text,
               Encoding.Unicode) { }

      public TextContent(bool     html,
                         string   text,
                         Encoding encoding)
      {
        Html     = html;
        Text     = text;
        Encoding = encoding;
      }

      #endregion




      #region Properties & Fields - Public

      public bool     Html     { get; set; }
      public string   Text     { get; set; }
      public Encoding Encoding { get; set; }

      #endregion




      #region Properties Impl - Public

      public ContentTypeEnum ContentType => Html ? ContentTypeEnum.Html : ContentTypeEnum.RawText;

      #endregion
    }

    [Serializable]
    public class ImageContent : IContent
    {
      #region Constructors

      public ImageContent(int              registryId,
                          ImageStretchType stretchType = ImageStretchType.Proportional)
      {
        RegistryId = registryId;
        StretchType = stretchType;
      }

      #endregion




      #region Properties & Fields - Public

      public int RegistryId { get; set; }

      public ImageStretchType StretchType { get; set; }

      #endregion




      #region Properties Impl - Public

      public ContentTypeEnum ContentType => ContentTypeEnum.Image;

      #endregion
    }
  }
}
