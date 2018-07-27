using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Interop.SuperMemo.UI.MainBar.Menu
{
  public interface IMainMenu : IMenu
  {
    IEditMenu EditMenu { get; }
  }
}
