using System;
using SuperMemoAssistant.Sys.IO.Devices;

namespace SuperMemoAssistant.Services.IO.Devices
{
  public interface IKeyboardHookService
  {
    event EventHandler<KeyboardHookEventArgs> KeyboardPressed;

    void RegisterHotKey(HotKey hotkey, Func<bool> callback);
    bool UnregisterHotKey(HotKey hotkey);
  }
}