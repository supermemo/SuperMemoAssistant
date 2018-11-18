using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Files
{
  [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 37)]
  public struct InfContentsElem
  {
    [FieldOffset(0)]
    /* 0x00 */ public byte     deleted;
    [FieldOffset(1)]
    /* 0x01 */ public int      parentId;
    [FieldOffset(5)]
    /* 0x05 */ public int      firstChildId;
    [FieldOffset(9)]
    /* 0x09 */ public int      lastChildId; //Unsure
    [FieldOffset(13)]
    /* 0x0D */ public int      prevSiblingId;
    [FieldOffset(17)]
    /* 0x11 */ public int      nextSiblingId;
    [FieldOffset(21)]
    /* 0x15 */ public int      descendantCount;
    [FieldOffset(25)]
    /* 0x19 */ public int      childrenCount;
    [FieldOffset(29)]
    /* 0x1D */ public int      unknown9;
    [FieldOffset(33)]
    /* 0x21 */ public int      unknown10;
    public static readonly int SizeOfContentsElem = Marshal.SizeOf(typeof(InfContentsElem));
  }
}
