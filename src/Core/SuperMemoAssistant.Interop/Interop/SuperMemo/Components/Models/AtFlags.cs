using System;

namespace SuperMemoAssistant.Interop.SuperMemo.Components.Models
{
  [Flags]
  public enum AtFlags
  {
    // TODO: Set correct values
    Browsing = 4,
    Editing = 8,
    Dragging = 16,
    Question = 32,
    Answer = 64,
    AfterGrading = 128,
  }
}
