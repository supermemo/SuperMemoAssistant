using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types
{
  public class Task : ElementBase, ITask
  {
    public Task(int id, InfContentsElem cttElem, InfElementsElem elElem)
      : base(id, cttElem, elElem)
    {
    }
  }
}
