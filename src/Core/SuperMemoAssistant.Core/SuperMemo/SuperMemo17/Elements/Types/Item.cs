using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types
{
  public class Item : ElementBase, IItem
  {
    public Item(int id, InfContentsElem cttElem, InfElementsElem elElem)
      : base(id, cttElem, elElem)
    {
    }
  }
}
