using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types
{
  public class ConceptGroup : ElementBase, IConceptGroup
  {
    public ConceptGroup(int id, InfContentsElem cttElem, InfElementsElem elElem)
      : base(id, cttElem, elElem)
    {
    }
  }
}
