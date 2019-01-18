using System;
using System.Collections.Generic;
using SuperMemoAssistant.Interop.SuperMemo.Core;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Components
{
  public interface IComponentRegistry
  {
    IComponentGroup this[int offset] { get; }
    IEnumerable<IComponentGroup> GetAll();
    int Count { get; }

    event Action<SMComponentGroupArgs> OnComponentGroupCreated;
    event Action<SMComponentGroupArgs> OnComponentGroupModified;
    event Action<SMComponentGroupArgs> OnComponentGroupDeleted;
  }
}
