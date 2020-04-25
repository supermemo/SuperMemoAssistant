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
// Created On:   2020/03/29 00:20
// Modified On:  2020/04/09 14:47
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.SuperMemo.Common.Content.Content
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Windows;
  using System.Windows.Media;
  using Anotar.Serilog;
  using Interop.SuperMemo.Content.Contents;
  using Layout.XamlControls;
  using SMA;
  using SuperMemoAssistant.Extensions;

  public static class ContentEx
  {
    #region Methods

    public static UIElement ToUIElement(this ContentBase content,
                                        int              id)
    {
      switch (content.ContentType)
      {
        case ContentTypeFlags.Html:
        case ContentTypeFlags.RawText:
          var textContent = (TextContent)content;
          return new XamlControlHtml(id, textContent.Text, content.DisplayAt);

        case ContentTypeFlags.Image:
          var imgContent = (ImageContent)content;
          var img        = Core.SM.Registry.Image[imgContent.RegistryId];

          // Display mode
          if (id != int.MinValue)
          {
            if (img == null) // || imgMember.Empty) // TODO: Why is Empty always true ?
            {
              LogTo.Error("Error while building XamlControlImage: IImage {RegistryId} is null. Skipping", imgContent.RegistryId);
              return null;
            }

            var imgFilePath = img.GetFilePath();

            if (File.Exists(imgFilePath) == false)
            {
              LogTo.Error("Error while building XamlControlImage: IImage {0} file {1} does not exist. Skipping",
                          img,
                          imgFilePath);
              return null;
            }
          }

          return new XamlControlImage(id, img, content.DisplayAt);

        case ContentTypeFlags.Sound:
          var soundContent = (SoundContent)content;
          var sound        = Core.SM.Registry.Sound[soundContent.RegistryId];

          // Display mode
          if (id != int.MinValue)
          {
            if (sound == null) // || imgMember.Empty) // TODO: Why is Empty always true ?
            {
              LogTo.Error("Error while building XamlControlSound: ISound {RegistryId} is null. Skipping", soundContent.RegistryId);
              return null;
            }

            var soundFilePath = sound.GetFilePath();

            if (File.Exists(soundFilePath) == false)
            {
              LogTo.Error("Error while building XamlControlSound: ISound {0} file {1} does not exist. Skipping",
                          sound,
                          soundFilePath);
              return null;
            }
          }

          return new XamlControlSound(id, sound,
                                      soundContent.Text,
                                      soundContent.PanelType,
                                      soundContent.PlayAt,
                                      content.DisplayAt);

        default:
          throw new NotImplementedException($"Content type {content.ContentType} not handled");
      }
    }

    public static Brush GetColorBrush(this ContentTypeFlags contentFlags)
    {
      List<Color> colors = new List<Color>();

      // Text
      if ((contentFlags & ContentTypeFlags.Text) != ContentTypeFlags.None)
        colors.Add(Colors.Gray.With(c => c.A = 100));

      // Image
      if ((contentFlags & ContentTypeFlags.Image) != ContentTypeFlags.None)
        colors.Add(Colors.LightGreen.With(c => c.A = 100));

      // Sound
      if ((contentFlags & ContentTypeFlags.Sound) != ContentTypeFlags.None)
        colors.Add(Colors.LightBlue.With(c => c.A = 100));

      return new SolidColorBrush(colors.Aggregate(Color.Add));
    }

    #endregion
  }
}
