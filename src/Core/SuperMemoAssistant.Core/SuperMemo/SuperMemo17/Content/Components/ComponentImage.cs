using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Content.Components
{
  public class ComponentImage : ComponentBase, IComponentImage
  {
    protected int ImageId { get; set; }

    public ComponentImage(ref InfComponentsImage comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt)
    {
      ImageId = SetValue(comp.registryId, nameof(ImageId));
      Stretch = SetValue((ImageStretchMode)comp.stretchType, nameof(Stretch));
    }

    public void Update(ref InfComponentsImage comp)
    {
      ComponentFieldFlags flags = ComponentFieldFlags.None;

      ImageId = SetValue(ImageId, comp.registryId, nameof(ImageId), ref flags);
      Stretch = SetValue(Stretch, (ImageStretchMode)comp.stretchType, nameof(Stretch), ref flags);

      base.Update(
        comp.left, comp.top,
        comp.width, comp.height,
        (AtFlags)comp.displayAt,
        flags
      );
    }

    public IImage Image => SMA.SMA.Instance.Registry.Image?[ImageId];
    public ImageStretchMode Stretch { get; set; }
  }
}
