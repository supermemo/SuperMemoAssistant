using SuperMemoAssistant.Interop.SuperMemo.Components.Models;
using SuperMemoAssistant.Interop.SuperMemo.Components.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Components.Types
{
  public class ComponentImage : ComponentBase, IComponentImage
  {
    protected int ImageId { get; set; }

    public ComponentImage(InfComponentsImage comp)
      : base(comp.left, comp.top, comp.right, comp.bottom, (AtFlags)comp.displayAt)
    {
      ImageId = SetValue(comp.registryId, nameof(ImageId));
      Stretch = SetValue((ImageStretchType)comp.stretchType, nameof(Stretch));
    }

    public void Update(InfComponentsImage comp)
    {
      ComponentFieldFlags flags = ComponentFieldFlags.None;

      ImageId = SetValue(ImageId, comp.registryId, nameof(ImageId), ref flags);
      Stretch = SetValue(Stretch, (ImageStretchType)comp.stretchType, nameof(Stretch), ref flags);

      base.Update(
        comp.left, comp.top,
        comp.right, comp.bottom,
        (AtFlags)comp.displayAt,
        flags
      );
    }

    public IImage Image => SMA.Instance.Registry.Image?[ImageId];
    public ImageStretchType Stretch { get; set; }
  }
}
