using System.Collections.Generic;
using SuperMemoAssistant.Sys.IO.Devices;

namespace SuperMemoAssistant.Services.IO.HotKeys
{
  public class HotKeyCfg
  {
    public Dictionary<string, HotKey> HotKeyMap { get; set; } = new Dictionary<string, HotKey>();
  }
}
