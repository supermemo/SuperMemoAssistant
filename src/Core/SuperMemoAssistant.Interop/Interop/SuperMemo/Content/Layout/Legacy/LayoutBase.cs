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
// Created On:   2019/01/16 14:54
// Modified On:  2019/01/16 15:32
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Anotar.Serilog;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Layout.Legacy
{
  public abstract class LayoutBase
  {

    #region Constructors

    protected LayoutBase(ContentTypeFlag acceptedContents)
    {
      AcceptedContents = acceptedContents;
    }


    #endregion




    #region Properties & Fields - Public

    public Rectangle Bounds   { get; set; }
    public int       Left   => Bounds.Left;
    public int       Right  => Bounds.Right;
    public int       Top    => Bounds.Top;
    public int       Bottom => Bounds.Bottom;
    public int       Width  => Bounds.Width;
    public int       Height => Bounds.Height;
    public abstract Size MinSize { get; }

    public ContentTypeFlag AcceptedContents { get; set; }

    #endregion




    public abstract void InitializeLayout();
    public abstract void CalculateLayout(Rectangle cors);

    public string Build(List<ContentBase> contents)
    {
      var components = new List<IComponent>();

      var corsCount = Contents.Count(c => c.Cors != null);

      if (corsCount > 0 && corsCount != Contents.Count())
        throw new ArgumentException("CORS must be set for all contents, or none.");

      var txtContents   = GetTextContents();
      var imgContents   = GetImageContents();
      var soundContents = GetSoundContents();

      switch (ElemBuilder.ContentType)
      {
        case ContentTypeFlag.Html:
        case ContentTypeFlag.RawText:
          if (imgContents.Any())
            throw new InvalidCastException("ElementBuilder ContentTypeFlag.Text contains non-text IContent");

          if (txtContents.Any() == false)
            throw new InvalidCastException("ElementBuilder ContentTypeFlag.Text does not contain any text IContent");

          BuildTextComponents(0,
                              txtContents.Count,
                              CorsFull.Left,
                              CorsFull.Top,
                              CorsFull.Right,
                              CorsFull.Bottom,
                              txtContents,
                              components);
          break;

        case ContentTypeFlag.Image:
          if (txtContents.Any())
            throw new InvalidCastException("ElementBuilder ContentTypeFlag.Image contains non-image IContent");

          if (imgContents.Any() == false)
            throw new InvalidCastException("ElementBuilder ContentTypeFlag.Image does not contain any image IContent");

          BuildImageComponents(0,
                               imgContents.Count,
                               CorsFull.Left,
                               CorsFull.Top,
                               CorsFull.Right,
                               CorsFull.Bottom,
                               imgContents,
                               components);
          break;

        case ContentTypeFlag.Sound:
          if (txtContents.Any())
            throw new InvalidCastException("ElementBuilder ContentTypeFlag.Image contains non-image IContent");

          if (imgContents.Any() == false)
            throw new InvalidCastException("ElementBuilder ContentTypeFlag.Image does not contain any image IContent");

          BuildImageComponents(0,
                               imgContents.Count,
                               CorsFull.Left,
                               CorsFull.Top,
                               CorsFull.Right,
                               CorsFull.Bottom,
                               imgContents,
                               components);
          break;

        case ContentTypeFlag.ImageAndRawText:
        case ContentTypeFlag.ImageAndHtml:
          if (txtContents.Any() == false)
            throw new InvalidCastException("ElementBuilder ContentTypeFlag.ImageAndText does not contain any text IContent");

          if (imgContents.Any() == false)
            throw new InvalidCastException("ElementBuilder ContentTypeFlag.ImageAndText does not contain any image IContent");

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
                           string.Join("\n",
                                       compsText));
    }





    #region Methods

    protected void BuildTextComponents(int                      startingIdx,
                                       int                      totalCompCount,
                                       int                      left,
                                       int                      top,
                                       int                      right,
                                       int                      bottom,
                                       IEnumerable<TextContent> contents,
                                       List<IComponent>         outComps)
    {
      int idx = 0;

      foreach (var content in contents)
      {
        GetCors(content,
                idx,
                totalCompCount,
                left,
                top,
                right,
                bottom,
                out int compLeft,
                out int compTop,
                out int compWidth,
                out int compHeight);

        var comp = new ComponentHtmlBuilder(startingIdx + idx++,
                                            content.Text,
                                            compLeft,
                                            compTop,
                                            compWidth,
                                            compHeight,
                                            content.DisplayAt);
        outComps.Add(comp);
      }
    }

    protected void BuildImageComponents(int                       startingIdx,
                                        int                       totalCompCount,
                                        int                       left,
                                        int                       top,
                                        int                       right,
                                        int                       bottom,
                                        IEnumerable<ImageContent> contents,
                                        List<IComponent>          outComps)
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

        GetCors(content,
                idx,
                totalCompCount,
                left,
                top,
                right,
                bottom,
                out int compLeft,
                out int compTop,
                out int compWidth,
                out int compHeight);

        var comp = new ComponentImageBuilder(startingIdx + idx++,
                                             img,
                                             compLeft,
                                             compTop,
                                             compWidth,
                                             compHeight,
                                             content.DisplayAt)
        {
          Stretch = content.StretchType
        };


        outComps.Add(comp);
      }
    }

    protected void BuildSoundComponents(int                       startingIdx,
                                        int                       totalCompCount,
                                        int                       left,
                                        int                       top,
                                        int                       right,
                                        int                       bottom,
                                        IEnumerable<SoundContent> contents,
                                        List<IComponent>          outComps)
    {
      int idx = 0;

      foreach (var content in contents)
      {
        var sound = Svc.SMA.Registry.Sound[content.RegistryId];

        if (sound == null) // || imgMember.Empty) // TODO: Why is Empty always true ?
        {
          LogTo.Error($"Error while building SoundComponent: ISound {content.RegistryId} is null. Skipping");
          continue;
        }

        var filePath = sound.GetFilePath();

        if (File.Exists(filePath) == false)
        {
          LogTo.Error("Error while building SoundComponent: ISound {0} file {1} does not exist. Skipping",
                      sound,
                      filePath);
          continue;
        }

        GetCors(content,
                idx,
                totalCompCount,
                left,
                top,
                right,
                bottom,
                out int compLeft,
                out int compTop,
                out int compWidth,
                out int compHeight);

        var comp = new ComponentSoundBuilder(startingIdx + idx++,
                                             sound,
                                             content.Text,
                                             compLeft,
                                             compTop,
                                             compWidth,
                                             compHeight,
                                             content.PanelType,
                                             content.PlayAt,
                                             content.DisplayAt);


        outComps.Add(comp);
      }
    }

    protected List<TextContent> GetTextContents()
    {
      return Contents.Where(c => (c.ContentType & ContentTypeFlag.Text) != ContentTypeFlag.None)
                     .Cast<TextContent>()
                     .ToList();
    }

    protected List<ImageContent> GetImageContents()
    {
      return Contents.Where(c => c.ContentType == ContentTypeFlag.Image)
                     .Cast<ImageContent>()
                     .ToList();
    }

    protected List<SoundContent> GetSoundContents()
    {
      return Contents.Where(c => c.ContentType == ContentTypeFlag.Sound)
                     .Cast<SoundContent>()
                     .ToList();
    }

    #endregion
  }
}
