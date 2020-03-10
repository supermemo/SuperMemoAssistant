using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.SMA.UI.Settings.Models
{
  /// <summary>
  /// Current status/operation being processed by the Plugin Manager
  /// </summary>
  public enum PluginManagerStatus
  {
    Error,
    Display,
    Refresh,
    Install,
    Uninstall,
    Update,
  }
}
