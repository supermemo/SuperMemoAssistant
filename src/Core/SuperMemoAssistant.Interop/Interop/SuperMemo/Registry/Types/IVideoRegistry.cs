using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Registry.Types
{
  public interface IVideoRegistry : IRegistry<IVideo>
  {
    /// <summary>
    /// Conveniency method. Will run UI automation to execute action.
    /// Add a new Video to the registry.
    /// </summary>
    /// <returns>Waitable task yielding success result of the operation</returns>
    Task<IVideo> AddAsync(string videoName, string videoPath);
  }
}
