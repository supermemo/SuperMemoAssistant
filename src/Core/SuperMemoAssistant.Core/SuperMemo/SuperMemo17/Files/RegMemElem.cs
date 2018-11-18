using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Files
{
  /// <summary>
  /// information about all registry members stored in the given registry, e.g.:
  /// - number of using elements,
  /// - pointer to the *.rtx file storing the name of the text registry member,
  /// - pointer to the *.lst file storing the list of elements using a given member,
  /// - etc.
  /// </summary>
  [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 30)]
  public unsafe struct RegMemElem
  {
    [FieldOffset(0)]
    public int useCount;
    [FieldOffset(4)]
    public byte linkType;
    [FieldOffset(5)]
    public byte unknown1;
    [FieldOffset(6)]
    public int unknown2;
    [FieldOffset(10)]
    public int rtxOffset;
    [FieldOffset(14)]
    public int rtxLength;
    [FieldOffset(18)]
    public int lstRelated; // Physical position ?
    [FieldOffset(22)]
    public int slotIdOrOffset; // Id: File ; Position: Rtf, Template (component), ...
    [FieldOffset(26)]
    public int slotLengthOrConceptGroup; // Rtf, Template (component), ...
    public static int SizeOfMemElem { get; } = Marshal.SizeOf(typeof(RegMemElem));
  }
}
