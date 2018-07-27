using System;

namespace SuperMemoAssistant.Interop.SuperMemo.Elements.Models
{
  [Flags]
  public enum ElementFieldFlags
  {
    None = 0,
    Parent = 1,
    NextSibling = 2,
    PrevSibling = 4,
    FirstChild = 8,
    LastChild = 16,
    DescendantCount = 32,
    ChildrenCount = 64,
    Name = 128,
    Components = 256,
    Template = 512,
    Concept = 1024,
    AFactor = 2048,
    Deleted = 4096,
  }
}
