using System;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.UI.MainBar.Menu;

namespace SuperMemoAssistant.Interop.SuperMemo.UI.MainBar
{
  public interface IMainBarWdw : IWdw
  {
    Task<bool> OpenMenu(Action<IMainMenu> action);
  }
}
