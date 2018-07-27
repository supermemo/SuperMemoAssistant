using System.Drawing;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Components.Types
{
  public interface IComponentSpelling : IComponent
  {
    IText Text { get; }
    Color Color { get; }
  }
}
