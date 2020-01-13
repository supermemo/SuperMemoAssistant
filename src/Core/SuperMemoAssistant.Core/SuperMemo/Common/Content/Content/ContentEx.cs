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
// Created On:   2019/05/08 19:51
// Modified On:  2019/08/08 11:16
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlControls;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Content
{
  public static class ContentEx
  {
    #region Methods

    public static UIElement ToUIElement(this ContentBase content,
                                        int              id)
    {
      switch (content.ContentType)
      {
        case ContentTypeFlag.Html:
        case ContentTypeFlag.RawText:
          var textContent = (TextContent)content;
          return new XamlControlHtml(id, textContent.Text, content.DisplayAt);

        case ContentTypeFlag.Image:
          var imgContent = (ImageContent)content;
          var img        = Core.SM.Registry.Image[imgContent.RegistryId];

          // Display mode
          if (id != int.MinValue)
          {
            if (img == null) // || imgMember.Empty) // TODO: Why is Empty always true ?
            {
              LogTo.Error($"Error while building XamlControlImage: IImage {imgContent.RegistryId} is null. Skipping");
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

        case ContentTypeFlag.Sound:
          var soundContent = (SoundContent)content;
          var sound        = Core.SM.Registry.Sound[soundContent.RegistryId];

          // Display mode
          if (id != int.MinValue)
          {
            if (sound == null) // || imgMember.Empty) // TODO: Why is Empty always true ?
            {
              LogTo.Error($"Error while building XamlControlSound: ISound {soundContent.RegistryId} is null. Skipping");
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

    public static Brush GetColorBrush(this ContentTypeFlag contentFlag)
    {
      List<Color> colors = new List<Color>();

      // Text
      if ((contentFlag & ContentTypeFlag.Text) != ContentTypeFlag.None)
        colors.Add(Colors.Gray.With(c => c.A = 100));

      // Image
      if ((contentFlag & ContentTypeFlag.Image) != ContentTypeFlag.None)
        colors.Add(Colors.LightGreen.With(c => c.A = 100));

      // Sound
      if ((contentFlag & ContentTypeFlag.Sound) != ContentTypeFlag.None)
        colors.Add(Colors.LightBlue.With(c => c.A = 100));

      return new SolidColorBrush(colors.Aggregate(Color.Add));
    }

    #endregion
  }
}
