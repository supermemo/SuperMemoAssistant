using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Components.Types
{
  public interface IComponentHtml : IComponent
  {
    IText Text { get; }
    bool IsFullHtml { get; }
  }
}
