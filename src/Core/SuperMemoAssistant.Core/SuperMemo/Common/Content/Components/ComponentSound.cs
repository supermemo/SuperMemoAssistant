using System;
using System.Drawing;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Components
{
  public class ComponentSound : ComponentBase, IComponentSound
  {
    protected int SoundId { get; set; }
    protected int ColorRed { get; set; }
    protected int ColorGreen { get; set; }
    protected int ColorBlue { get; set; }

    public ComponentSound(ref InfComponentsSound17 comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt)
    {
      SoundId = SetValue(comp.registryId, nameof(SoundId));
      ColorRed = SetValue(comp.colorRed, nameof(ColorRed));
      ColorGreen = SetValue(comp.colorGreen, nameof(ColorGreen));
      ColorBlue = SetValue(comp.colorBlue, nameof(ColorBlue));
      PlayAt = SetValue((AtFlags)comp.playAt, nameof(PlayAt));
      ExtractStart = SetValue(comp.extractStart, nameof(ExtractStart));
      ExtractStop = SetValue(comp.extractStop, nameof(ExtractStop));
      IsContinuous = SetValue(comp.isContinuous, nameof(IsContinuous));
      Panel = SetValue((MediaPanelType)comp.panel, nameof(Panel));
      TextAlignment = SetValue((TextAlignment)comp.textAlignment, nameof(TextAlignment));
    }

    public void Update(ref InfComponentsSound17 comp)
    {
      ComponentFieldFlags flags = ComponentFieldFlags.None;

      SoundId = SetValue(SoundId, comp.registryId, nameof(SoundId), ref flags);
      ColorRed = SetValue(ColorRed, comp.colorRed, nameof(ColorRed), ref flags);
      ColorGreen = SetValue(ColorGreen, comp.colorGreen, nameof(ColorGreen), ref flags);
      ColorBlue = SetValue(ColorBlue, comp.colorBlue, nameof(ColorBlue), ref flags);
      PlayAt = SetValue(PlayAt, (AtFlags)comp.playAt, nameof(PlayAt), ref flags);
      ExtractStart = SetValue(ExtractStart, comp.extractStart, nameof(ExtractStart), ref flags);
      ExtractStop = SetValue(ExtractStop, comp.extractStop, nameof(ExtractStop), ref flags);
      IsContinuous = SetValue(IsContinuous, comp.isContinuous, nameof(IsContinuous), ref flags);
      Panel = SetValue(Panel, (MediaPanelType)comp.panel, nameof(Panel), ref flags);
      TextAlignment = SetValue(TextAlignment, (TextAlignment)comp.textAlignment, nameof(TextAlignment), ref flags);

      base.Update(
        comp.left, comp.top,
        comp.width, comp.height,
        (AtFlags)comp.displayAt,
        flags
      );
    }

    public ISound Sound => throw new NotImplementedException();
    public AtFlags PlayAt { get; set; }
    public uint ExtractStart { get; set; }
    public uint ExtractStop { get; set; }
    public bool IsContinuous { get; set; }
    public MediaPanelType Panel { get; set; }
    public Color Color => Color.FromArgb(ColorRed, ColorGreen, ColorBlue);
    public TextAlignment TextAlignment { get; set; }
  }
}
