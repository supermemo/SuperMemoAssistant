using SuperMemoAssistant.Interop.SuperMemo.Components.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Components.Types
{
  public interface IComponentImage
  {
    IImage Image { get; }
    ImageStretchType Stretch { get; }
  }
}
