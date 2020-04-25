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
// Modified On:  2020/04/09 15:38
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.SuperMemo.Common.Content.Components
{
  using System;
  using System.Drawing;
  using Interop.SuperMemo.Content.Components;
  using Interop.SuperMemo.Content.Models;
  using Interop.SuperMemo.Registry.Members;
  using SuperMemo17.Files;

  internal class ComponentSound : ComponentBase, IComponentSound
  {
    #region Properties & Fields - Non-Public

    protected int SoundId    { get; set; }
    protected int ColorRed   { get; set; }
    protected int ColorGreen { get; set; }
    protected int ColorBlue  { get; set; }

    #endregion




    #region Constructors

    public ComponentSound(ref InfComponentsSound17 comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt)
    {
      SoundId       = SetValue(comp.registryId, nameof(SoundId));
      ColorRed      = SetValue(comp.colorRed, nameof(ColorRed));
      ColorGreen    = SetValue(comp.colorGreen, nameof(ColorGreen));
      ColorBlue     = SetValue(comp.colorBlue, nameof(ColorBlue));
      PlayAt        = SetValue((AtFlags)comp.playAt, nameof(PlayAt));
      ExtractStart  = SetValue(comp.extractStart, nameof(ExtractStart));
      ExtractStop   = SetValue(comp.extractStop, nameof(ExtractStop));
      IsContinuous  = SetValue(comp.isContinuous, nameof(IsContinuous));
      Panel         = SetValue((MediaPanelType)comp.panel, nameof(Panel));
      TextAlignment = SetValue((TextAlignment)comp.textAlignment, nameof(TextAlignment));
    }

    #endregion




    #region Properties Impl - Public

    public ISound         Sound         => throw new NotImplementedException();
    public AtFlags        PlayAt        { get; set; }
    public uint           ExtractStart  { get; set; }
    public uint           ExtractStop   { get; set; }
    public bool           IsContinuous  { get; set; }
    public MediaPanelType Panel         { get; set; }
    public Color          Color         => Color.FromArgb(ColorRed, ColorGreen, ColorBlue);
    public TextAlignment  TextAlignment { get; set; }

    #endregion




    #region Methods

    public void Update(ref InfComponentsSound17 comp)
    {
      ComponentFieldFlags flags = ComponentFieldFlags.None;

      SoundId       = SetValue(SoundId, comp.registryId, nameof(SoundId), ref flags);
      ColorRed      = SetValue(ColorRed, comp.colorRed, nameof(ColorRed), ref flags);
      ColorGreen    = SetValue(ColorGreen, comp.colorGreen, nameof(ColorGreen), ref flags);
      ColorBlue     = SetValue(ColorBlue, comp.colorBlue, nameof(ColorBlue), ref flags);
      PlayAt        = SetValue(PlayAt, (AtFlags)comp.playAt, nameof(PlayAt), ref flags);
      ExtractStart  = SetValue(ExtractStart, comp.extractStart, nameof(ExtractStart), ref flags);
      ExtractStop   = SetValue(ExtractStop, comp.extractStop, nameof(ExtractStop), ref flags);
      IsContinuous  = SetValue(IsContinuous, comp.isContinuous, nameof(IsContinuous), ref flags);
      Panel         = SetValue(Panel, (MediaPanelType)comp.panel, nameof(Panel), ref flags);
      TextAlignment = SetValue(TextAlignment, (TextAlignment)comp.textAlignment, nameof(TextAlignment), ref flags);

      base.Update(
        comp.left, comp.top,
        comp.width, comp.height,
        (AtFlags)comp.displayAt,
        flags
      );
    }

    #endregion
  }
}
