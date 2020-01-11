using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Components
{
  public class ComponentImage : ComponentBase, IComponentImage
  {
    protected int ImageId { get; set; }

    public ComponentImage(ref InfComponentsImage17 comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt)
    {
      ImageId = SetValue(comp.registryId, nameof(ImageId));
      Stretch = SetValue((ImageStretchMode)comp.stretchType, nameof(Stretch));
    }

    public void Update(ref InfComponentsImage17 comp)
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

    public IImage Image => Core.SM.Registry.Image?[ImageId];
    public ImageStretchMode Stretch { get; set; }
  }
}
