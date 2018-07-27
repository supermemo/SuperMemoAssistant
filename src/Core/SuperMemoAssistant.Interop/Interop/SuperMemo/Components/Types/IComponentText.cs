using System.Drawing;
using SuperMemoAssistant.Interop.SuperMemo.Components.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Components.Types
{
  public interface IComponentText : IComponent
  {
    IText Text { get; }
    TextAlignment TextAlignment { get; }
    Color Color { get; }
  }
}
