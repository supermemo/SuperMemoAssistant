using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Registry.Types
{
  public interface ITextRegistry : IRegistry<IText>
  {
    /// <summary>
    /// Conveniency method. Will run UI automation to execute action.
    /// Add a new Concept to the registry.
    /// </summary>
    /// <returns>Waitable task yielding success result of the operation</returns>
    Task<IText> AddAsync(string text);
  }
}
