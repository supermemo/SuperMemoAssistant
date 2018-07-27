using System.Collections.Generic;

namespace SuperMemoAssistant.Interop.SuperMemo.Registry.Types
{
  public interface IRegistry<out IType> : IEnumerable<IType>
  {
    int Count { get; }

    /// <summary>
    /// Retrieve registry element from memory at given <paramref name="index"/>
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Element or null if invalid index (deleted, out of bound, ...)</returns>
    IType this[int id] { get; }
  }
}
