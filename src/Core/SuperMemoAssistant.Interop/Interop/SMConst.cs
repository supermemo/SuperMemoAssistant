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
// Created On:   2019/09/03 18:15
// Modified On:  2020/01/13 21:02
// Modified By:  Alexis

#endregion




using System.Windows.Media;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.Interop
{
  public static class SMConst
  {
    #region Constants & Statics

    public const string AppName = "SuperMemoAssistant";

    #endregion




    public static class Stylesheet
    {
      #region Constants & Statics

      public static readonly Color
        ExtractClozedColor = (Color)ColorConverter.ConvertFromString("#E67300"); // ColorTranslator.FromHtml("#E67300");
      public static readonly Color ExtractColor            = (Color)ColorConverter.ConvertFromString("#44C2FF");
      public static readonly Color ExtractTransparentColor = (Color)ColorConverter.ConvertFromString("#8044C2FF");
      public static readonly Color IgnoreColor             = (Color)ColorConverter.ConvertFromString("#DAB6B6");

      #endregion
    }


    public static class Elements
    {
      #region Constants & Statics

      public const int DefaultChildrenPerNode = 100;

      public const string ReferenceFormat =
        @"<br><br><hr SuperMemo><SuperMemoReference><H5 dir=ltr align=left><FONT style=""COLOR: transparent"" size=1>#SuperMemo Reference:</FONT><BR><FONT class=reference>{0}</FONT></SuperMemoReference>";

      #endregion
    }


    public static class Paths
    {
      #region Constants & Statics

      public const string ElementsFolder = "elements";
      public const string InfoFolder     = "info";
      public const string RegistryFolder = "registry";

      #endregion
    }

    public static class Files
    {
      #region Constants & Statics

      public const string BinaryMemFileName    = "program.mem";
      public const string BinaryRtxFileName    = "program.rtx";
      public const string ConceptMemFileName   = "concept.mem";
      public const string ConceptRtxFileName   = "concept.rtx";
      public const string ContentsFileName     = "contents.dat";
      public const string ElementsInfoFileName = "elementinfo.dat";
      public const string ImageMemFileName     = "image.mem";
      public const string ImageRtxFileName     = "image.rtx";
      public const string SoundMemFileName     = "sound.mem";
      public const string SoundRtxFileName     = "sound.rtx";
      public const string TemplateMemFileName  = "template.mem";
      public const string TemplateRtxFileName  = "template.rtx";
      public const string TextMemFileName      = "text.mem";
      public const string TextRtfFileName      = "text.rtf";
      public const string TextRtxFileName      = "text.rtx";
      public const string VideoMemFileName     = "video.mem";
      public const string VideoRtxFileName     = "video.rtx";

      #endregion
    }

    public static class UI
    {
      #region Constants & Statics

      public const string ElementDataWindowClassName = "TElDataWind";
      public const string ElementWindowClassName     = "TElWind";
      public const string SMMainClassName            = "TSMMain";

      public const string MainMenuItemClassName = "#32768";

      #endregion
    }
  }
}
