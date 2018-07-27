using SuperMemoAssistant.Interop.SuperMemo.Components.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Components.Types
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
