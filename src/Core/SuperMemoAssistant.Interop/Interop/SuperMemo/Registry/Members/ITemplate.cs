using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Components;

namespace SuperMemoAssistant.Interop.SuperMemo.Registry.Members
{
  public interface ITemplate : IRegistryMember
  {
    IComponentGroup GetComponentGroup();
  }
}
