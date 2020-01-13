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
// Created On:   2019/02/26 23:18
// Modified On:  2019/03/01 20:58
// Modified By:  Alexis

#endregion




using System.ComponentModel;
using System.Drawing;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using Size = System.Windows.Size;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlControls
{
  /// <summary>Interaction logic for XamlControlSound.xaml</summary>
  public partial class XamlControlSound : XamlControlBase, IComponentSound, INotifyPropertyChanged
  {
    #region Constants & Statics

    public static readonly Size MinSize = new Size(2000,
                                                   800);

    #endregion




    #region Constructors

    public XamlControlSound(int            id,
                            ISound         sound,
                            string         text,
                            MediaPanelType panelType,
                            AtFlags        playAt,
                            AtFlags        displayAt)
    {
      Id        = id;
      Sound     = sound;
      Text      = text;
      Panel     = panelType;
      PlayAt    = playAt;
      DisplayAt = displayAt;

      DataContext = this;

      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public int    Id   { get; }
    public string Text { get; set; }

    #endregion




    #region Properties Impl - Public

    public override AtFlags DisplayAt { get; }

    public ISound         Sound         { get; set; }
    public AtFlags        PlayAt        { get; set; }
    public uint           ExtractStart  { get; set; }
    public uint           ExtractStop   { get; set; }
    public bool           IsContinuous  { get; set; }
    public MediaPanelType Panel         { get; set; }
    public Color          Color         { get; set; }
    public TextAlignment  TextAlignment { get; set; }

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      return $@"Begin Component #{Id + 1}
Type=Sound
Cors=({Left},{Top},{Width},{Height})
DisplayAt={(int)DisplayAt}
PlayAt={(int)PlayAt}
Hyperlink=0
SoundName={Sound.Name}
SoundFile={Sound.GetFilePath()}
Text={Text}
Center=0
Color=-1
StartPosition=0
EndPosition=99999999
Panel={(int)Panel}
Continuous=0
Transparent=0
End Component #{Id + 1}";
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
