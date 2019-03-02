using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;

namespace SuperMemoAssistant.Services.UI.Configuration.ElementPicker {
  public interface IElementPickerCallback
  {
    void SetElement(IElement elem);
  }
}