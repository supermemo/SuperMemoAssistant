using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;

namespace SuperMemoAssistant.Interop.SuperMemo.Registry.Members
{
  public interface IConcept : IRegistryMember
  {
    IConceptGroup ConceptGroup { get; }
  }
}
