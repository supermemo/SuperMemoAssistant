using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Components
{
  public interface IComponentVideo : IComponent
  {
    IVideo Video { get; }
    bool IsContinuous { get; }
    bool IsFullScreen { get; }
    uint ExtractStart { get; }
    uint ExtractStop { get; }
    MediaPanelType Panel { get; }
  }
}
