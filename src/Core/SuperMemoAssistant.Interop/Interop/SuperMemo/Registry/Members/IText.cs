using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Interop.SuperMemo.Registry.Members
{
  public interface IText : IRegistryMember
  {
    string GetContent();
  }
}
