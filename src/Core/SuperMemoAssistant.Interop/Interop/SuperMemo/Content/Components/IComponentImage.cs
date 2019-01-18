using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Components
{
  public interface IComponentImage : IComponent
  {
    IImage Image { get; }
    ImageStretchMode Stretch { get; }
  }
}
