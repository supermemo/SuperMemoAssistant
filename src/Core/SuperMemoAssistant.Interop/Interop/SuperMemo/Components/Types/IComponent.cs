using SuperMemoAssistant.Interop.SuperMemo.Components.Models;

namespace SuperMemoAssistant.Interop.SuperMemo.Components.Types
{
  public interface IComponent
  {
    short Left { get; }
    short Top { get; }
    short Right { get; }
    short Bottom { get; }
    AtFlags DisplayAt { get; }
  }
}
