using SuperMemoAssistant.Interop.SuperMemo.UI.MainBar.Menu;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.UI.MainBar.Menu
{
  public class MainMenu : MenuRoot, IMainMenu
  {
    #region Constructors

    public MainMenu(WdwBase parent)
      : base(parent) { }

    #endregion




    #region Properties Impl

    public IEditMenu EditMenu => Expand(new EditMenu(Parent));

    #endregion
  }
}
