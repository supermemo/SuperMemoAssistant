using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Interop.SuperMemo.UI.MainBar.Menu
{
  public interface IEditMenusMenu : IMenu
  {
    // TODO: Return actual menus
    void Elements();
    void Components();
    void Contents();
    void Browser();
    void Registry();
  }
}
