using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Registry.Types
{
  public interface ISoundRegistry : IRegistry<ISound>
  {
    /// <summary>
    /// Conveniency method. Will run UI automation to execute action.
    /// Add a new Sound to the registry.
    /// </summary>
    /// <returns>Waitable task yielding success result of the operation</returns>
    Task<ISound> AddAsync(string soundName, string soundPath);
  }
}
