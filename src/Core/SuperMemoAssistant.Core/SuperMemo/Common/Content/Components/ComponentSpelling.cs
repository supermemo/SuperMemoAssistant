using System.Drawing;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Components
{
  public class ComponentSpelling : ComponentBase, IComponentSpelling
  {
    protected int TextId { get; set; }
    protected int ColorRed { get; set; }
    protected int ColorGreen { get; set; }
    protected int ColorBlue { get; set; }

    public ComponentSpelling(ref InfComponentsSpelling17 comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt)
    {
      TextId = SetValue(comp.registryId, nameof(TextId));
      ColorRed = SetValue(comp.colorRed, nameof(ColorRed));
      ColorGreen = SetValue(comp.colorGreen, nameof(ColorGreen));
      ColorBlue = SetValue(comp.colorBlue, nameof(ColorBlue));
    }

    public void Update(ref InfComponentsSpelling17 comp)
    {
      ComponentFieldFlags flags = ComponentFieldFlags.None;

      TextId = SetValue(TextId, comp.registryId, nameof(TextId), ref flags);
      ColorRed = SetValue(ColorRed, comp.colorRed, nameof(ColorRed), ref flags);
      ColorGreen = SetValue(ColorGreen, comp.colorGreen, nameof(ColorGreen), ref flags);
      ColorBlue = SetValue(ColorBlue, comp.colorBlue, nameof(ColorBlue), ref flags);

      base.Update(
        comp.left, comp.top,
        comp.width, comp.height,
        (AtFlags)comp.displayAt,
        flags
      );
    }

    public IText Text => Core.SM.Registry.Text?[TextId];
    public Color Color => Color.FromArgb(ColorRed, ColorGreen, ColorBlue);
  }
}
