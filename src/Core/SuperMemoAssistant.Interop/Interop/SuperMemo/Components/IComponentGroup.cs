using System;
using System.Collections.Generic;
using SuperMemoAssistant.Interop.SuperMemo.Components.Types;
using SuperMemoAssistant.Interop.SuperMemo.Core;

namespace SuperMemoAssistant.Interop.SuperMemo.Components
{
  public interface IComponentGroup
  {
    IComponent this[int idx] { get; }
    IEnumerable<IComponent> Components { get; }

    int Count { get; }
    int Offset { get; }



    //
    // Events

    event Action<SMComponentGroupArgs> OnChanged;
  }
}
