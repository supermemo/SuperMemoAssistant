using System.Drawing;
using SuperMemoAssistant.Interop.SuperMemo.Components.Models;
using SuperMemoAssistant.Interop.SuperMemo.Components.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Components.Types
{
  public class ComponentSpelling : ComponentBase, IComponentSpelling
  {
    protected int TextId { get; set; }
    protected int ColorRed { get; set; }
    protected int ColorGreen { get; set; }
    protected int ColorBlue { get; set; }

    public ComponentSpelling(InfComponentsSpelling comp)
      : base(comp.left, comp.top, comp.right, comp.bottom, (AtFlags)comp.displayAt)
    {
      TextId = SetValue(comp.registryId, nameof(TextId));
      ColorRed = SetValue(comp.colorRed, nameof(ColorRed));
      ColorGreen = SetValue(comp.colorGreen, nameof(ColorGreen));
      ColorBlue = SetValue(comp.colorBlue, nameof(ColorBlue));
    }

    public void Update(InfComponentsSpelling comp)
    {
      ComponentFieldFlags flags = ComponentFieldFlags.None;

      TextId = SetValue(TextId, comp.registryId, nameof(TextId), ref flags);
      ColorRed = SetValue(ColorRed, comp.colorRed, nameof(ColorRed), ref flags);
      ColorGreen = SetValue(ColorGreen, comp.colorGreen, nameof(ColorGreen), ref flags);
      ColorBlue = SetValue(ColorBlue, comp.colorBlue, nameof(ColorBlue), ref flags);

      base.Update(
        comp.left, comp.top,
        comp.right, comp.bottom,
        (AtFlags)comp.displayAt,
        flags
      );
    }

    public IText Text => SMA.Instance.Registry.Text?[TextId];
    public Color Color => Color.FromArgb(ColorRed, ColorGreen, ColorBlue);
  }
}
