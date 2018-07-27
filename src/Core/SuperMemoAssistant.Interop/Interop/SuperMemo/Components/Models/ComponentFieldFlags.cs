using System;

namespace SuperMemoAssistant.Interop.SuperMemo.Components.Models
{
  [Flags]
  public enum ComponentFieldFlags : long
  {
    None = 0,
    Left = 1,
    Top = 2,
    Right = 4,
    Bottom = 8,
    DisplayAt = 16,
    RegistryId = 32,
    Color = 64,
    TextAlignment = 128,
    ExtractStart = 256,
    ExtractStop = 512,
    PanelType = 1024,
    PlayAt = 2048,
    IsContinuous = 4096,
    IsFullScreen = 8192,
    IsFullHtml = 16384,
    StretchType = 32768
  }
}
