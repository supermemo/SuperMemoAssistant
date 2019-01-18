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
// Created On:   2019/01/18 16:04
// Modified On:  2019/01/18 16:09
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Services;
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Layout.XamlControls
{
  /// <summary>Interaction logic for XamlControlGroup.xaml</summary>
  public partial class XamlControlGroup : UserControl
  {
    #region Constants & Statics

    private const string ComponentsSkeleton = @"ComponentNo={0}
{1}";

    #endregion




    #region Properties & Fields - Non-Public

    private List<(Panel, ContentTypeFlag)>     PanelAcceptedContentList { get; } = new List<(Panel, ContentTypeFlag)>();
    private Dictionary<ContentTypeFlag, Panel> ContentPanelMap          { get; } = new Dictionary<ContentTypeFlag, Panel>();

    private List<UIElement> ContentControls { get; } = new List<UIElement>();

    #endregion




    #region Constructors

    /// <inheritdoc />
    public XamlControlGroup()
    {
      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public bool            IsValid         { get; private set; } = true;
    public ContentTypeFlag AcceptedContent { get; private set; }

    #endregion




    #region Methods

    public void LoadXaml(string xaml)
    {
      PanelAcceptedContentList.Clear();
      ContentPanelMap.Clear();

      IsValid = true;
      Content = null;

      var context = new ParserContext { XamlTypeMapper = new XamlTypeMapper(new string[] { }) };

      var @namespace   = GetType().Namespace;
      var assemblyName = GetType().Assembly.FullName;

      context.XmlnsDictionary.Add("sma", $"clr-namespace:{@namespace}");
      // ReSharper disable once AssignNullToNotNullAttribute
      context.XamlTypeMapper.AddMappingProcessingInstruction($"clr-namespace:{@namespace}", @namespace, assemblyName);

      DependencyObject rootObject = XamlReader.Parse(xaml, context) as DependencyObject;

      if (!(rootObject is UIElement rootUIELement))
        throw new ArgumentException("Invalid xaml");

      Content = rootUIELement;
      
      var size = new Size(10000, 10000);
      Measure(size);
      Arrange(new Rect(size));
      UpdateLayout();

      FindContentContainers();
      CreateContentPanelMap();
      
      ValidateAcceptedContents();
      SetupContentColors();

      AcceptedContent = PanelAcceptedContentList
                        .Select(pc => pc.Item2)
                        .Aggregate((c1,
                                    c2) => c1 | c2);
    }

    public void Unload()
    {
      PanelAcceptedContentList.Clear();
      ContentPanelMap.Clear();

      AcceptedContent = ContentTypeFlag.None;
      IsValid         = true;

      Content = null;
    }

    public string ToString(List<ContentBase> contents)
    {
      if (!IsValid)
        return null;
      
      AddContent(contents);

      //var size = new Size(10000, 10000);
      //Measure(size);
      //Arrange(new Rect(size));
      UpdateLayout();

      var ret = string.Format(
        ComponentsSkeleton,
        ContentControls.Count,
        string.Join("\n", ContentControls.Select(c => c.ToString())));

      ClearContent();

      return ret;
    }

    public void AddContent(List<ContentBase> contents)
    {
      if (!IsValid)
        return;

      int id = 0;

      foreach (var content in contents)
      {
        var panel = ContentPanelMap.SafeGet(content.ContentType);

        if (panel == null)
          continue;

        var uiElement = ContentToUIElement(content, id++);

        if (uiElement == null)
          continue;

        panel.Children.Add(uiElement);
        ContentControls.Add(uiElement);
      }
    }

    public void ClearContent()
    {
      foreach (var pc in PanelAcceptedContentList)
        pc.Item1.Children.Clear();

      ContentControls.Clear();
    }

    private UIElement ContentToUIElement(ContentBase content,
                                         int         id)
    {
      switch (content.ContentType)
      {
        case ContentTypeFlag.Html:
        case ContentTypeFlag.Text:
          var textContent = (TextContent)content;
          return new XamlControlHtml(id, textContent.Text, content.DisplayAt);

        case ContentTypeFlag.Image:
          var imgContent = (ImageContent)content;
          var img        = Svc.SMA.Registry.Image[imgContent.RegistryId];
          
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

          return new XamlControlImage(id, img, content.DisplayAt);

        case ContentTypeFlag.Sound:
          var soundContent = (SoundContent)content;
          var sound        = Svc.SMA.Registry.Sound[soundContent.RegistryId];
          
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

          return new XamlControlSound(id, sound,
                                      soundContent.Text,
                                      soundContent.PanelType,
                                      soundContent.PlayAt,
                                      content.DisplayAt);

        default:
          throw new NotImplementedException($"Content type {content.ContentType} not handled");
      }
    }

    private void CreateContentPanelMap()
    {
      foreach (var pc in PanelAcceptedContentList)
      {
        for (int cf = (int)pc.Item2, shift = 0;
             cf > 0;
             cf >>= 1, shift++)
        {
          var c = cf & 0x0001;

          if (c > 0)
            ContentPanelMap[(ContentTypeFlag)(c << shift)] = pc.Item1;
        }
      }
    }

    private void FindContentContainers()
    {
      foreach (var child in this.EnumerateChildren())
      {
        if (!(child is Panel panel))
          continue;

        var acceptedContent = child.ReadLocalValue(Container.AcceptedContentProperty);

        if (acceptedContent == DependencyProperty.UnsetValue || acceptedContent.GetType() != Container.AcceptedContentProperty.PropertyType)
          continue;

        PanelAcceptedContentList.Add((panel, (ContentTypeFlag)acceptedContent));
      }
    }

    private void ValidateAcceptedContents()
    {
      for (int i = 0; i < PanelAcceptedContentList.Count; i++)
      for (int j = 0; j < PanelAcceptedContentList.Count; j++)
      {
        if (i == j)
          continue;

        var (p1, ac1) = PanelAcceptedContentList[i];
        var (p2, ac2) = PanelAcceptedContentList[j];

        if (ac1 == ContentTypeFlag.None || ac2 == ContentTypeFlag.None)
          continue;

        if ((ac1 & ac2) == ContentTypeFlag.None)
          continue;

        p1.Background = p2.Background = Brushes.Red;
        IsValid       = false;
      }
    }

    private void SetupContentColors()
    {
      foreach (var pc in PanelAcceptedContentList)
      {
        if (pc.Item1.Background is SolidColorBrush)
          continue;

        pc.Item1.Background = ContentToBrush(pc.Item2);
      }
    }

    private Brush ContentToBrush(ContentTypeFlag contentFlag)
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
