using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Interop.SuperMemo.UI.MainBar.Menu
{
  public interface IEditMenu : IMenu
  {
    IEditMenusMenu MenusMenu { get; }
  }
}
