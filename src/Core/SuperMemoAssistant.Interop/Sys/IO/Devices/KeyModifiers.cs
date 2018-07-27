using System;

namespace SuperMemoAssistant.Sys.IO.Devices {
  [Flags]
  [Serializable]
  public enum KeyModifiers
  {
    None     = 0,
    Alt      = 1,
    Ctrl     = 2,
    Shift    = 4,
    Win      = 8,
    NoRepeat = 0x4000,
  }
}