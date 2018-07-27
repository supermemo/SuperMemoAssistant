using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Members
{
  public class Concept : RegistryMemberBase, IConcept
  {
    public Concept(int id, RegMemElem mem, RegRtElem rt)
      : base(id, mem, rt)
    {

    }

    public string Name => RtxValue;

    public IConceptGroup ConceptGroup => (IConceptGroup)SMA.Instance.Registry.Element?[SlotLengthOrConceptGroupId];

    public string GetFilePath()
    {
      return null;
    }

    public Task<bool> DeleteAsync()
    {
      throw new NotImplementedException();
    }

    public IEnumerable<IElement> GetLinkedElements()
    {
      throw new NotImplementedException();
    }

    public Task<bool> NeuralAsync()
    {
      throw new NotImplementedException();
    }

    public Task<bool> RenameAsync(string newName)
    {
      throw new NotImplementedException();
    }
  }
}
