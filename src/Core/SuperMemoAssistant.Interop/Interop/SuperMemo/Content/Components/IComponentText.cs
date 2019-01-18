using System.Drawing;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Components
{
  public interface IComponentText : IComponent
  {
    IText Text { get; }
    TextAlignment TextAlignment { get; }
    Color Color { get; }
  }
}
