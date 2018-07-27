using System.Collections.Generic;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;

namespace SuperMemoAssistant.Interop.SuperMemo.Registry.Members
{
  public interface IRegistryMember
  {
    int Id { get; }
    string Name { get; }

    /// <summary>
    /// Retrieve linked file path (HTML, Image, Audio, ...)
    /// </summary>
    /// <returns>File path or null</returns>
    string GetFilePath();

    /// <summary>
    /// Retrieve elements that are using this registry 
    /// </summary>
    /// <returns></returns>
    IEnumerable<IElement> GetLinkedElements();

    /// <summary>
    /// Conveniency method. Will run UI automation to execute action.
    /// Delete current member from its registry.
    /// </summary>
    /// <returns>Waitable task yielding success result of the operation</returns>
    Task<bool> DeleteAsync();

    /// <summary>
    /// Conveniency method. Will run UI automation to execute action.
    /// Rename current member in registry with <paramref name="newName"/>.
    /// </summary>
    /// <returns>Waitable task yielding success result of the operation</returns>
    Task<bool> RenameAsync(string newName);

    /// <summary>
    /// Conveniency method. Will run UI automation to execute action.
    /// Starts a neural review on given registry member.
    /// </summary>
    /// <returns>Waitable task yielding success result of the operation</returns>
    Task<bool> NeuralAsync();
  }
}
