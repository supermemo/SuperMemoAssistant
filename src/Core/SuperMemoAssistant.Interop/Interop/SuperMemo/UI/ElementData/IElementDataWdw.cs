using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;

namespace SuperMemoAssistant.Interop.SuperMemo.UI.ElementData
{
  public interface IElementDataWdw : IWdw
  {
    int CurrentElementId { get; }
    IElement CurrentElement { get; }

    event Action<SMElementArgs> OnElementChanged;
  }
}
