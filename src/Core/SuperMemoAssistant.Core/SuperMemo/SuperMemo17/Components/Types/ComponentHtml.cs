using SuperMemoAssistant.Interop.SuperMemo.Components.Models;
using SuperMemoAssistant.Interop.SuperMemo.Components.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Components.Types
{
  public class ComponentHtml : ComponentBase, IComponentHtml
  {
    protected int TextId { get; set; }
    protected int ColorRed { get; set; }
    protected int ColorGreen { get; set; }
    protected int ColorBlue { get; set; }

    public ComponentHtml(InfComponentsHtml comp)
      : base(comp.left, comp.top, comp.right, comp.bottom, (AtFlags)comp.displayAt)
    {
      TextId = SetValue(comp.registryId, nameof(TextId));
      IsFullHtml = SetValue(comp.isFullHtml != 0, nameof(IsFullHtml));
    }

    public void Update(InfComponentsHtml comp)
    {
      ComponentFieldFlags flags = ComponentFieldFlags.None;

      TextId = SetValue(TextId, comp.registryId, nameof(TextId), ref flags);
      IsFullHtml = SetValue(IsFullHtml, comp.isFullHtml != 0, nameof(IsFullHtml), ref flags);

      base.Update(
        comp.left, comp.top,
        comp.right, comp.bottom,
        (AtFlags)comp.displayAt,
        flags
      );
    }

    public IText Text => SMA.Instance.Registry.Text?[TextId];
    public bool IsFullHtml { get; set; }
  }
}
