#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   2018/05/19 14:02
// Modified On:  2022/12/17 14:09
// Modified By:  - Alexis
//               - Ki

#endregion




using System.Runtime.InteropServices;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Files
{
  internal class InfElementsElemContainer17
  {
    #region Constructors

    public InfElementsElemContainer17(InfElementsElem17 e)
    {
      _elem = e;
    }

    #endregion




    #region Properties & Fields - Public

    public InfElementsElem17 _elem;

    #endregion
  }

  [StructLayout(LayoutKind.Explicit,
    Pack = 1,
    Size = 118)]
  internal unsafe struct InfElementsElem17
  {
    [FieldOffset(0)]
    /* 0x00 */ public byte elementType; // (00: Topic/Deleted, 01: Item, 02: ?, 03: ?, 04: Concept)
    [FieldOffset(1)]
    /* 0x01 */ public byte unknownbyte1;
    [FieldOffset(2)]
    /* 0x02 */ public int titleTextId; // id / -1 if deleted
    [FieldOffset(6)]
    /* 0x06 */ public int componPos; // FF FF FF FF if none
    [FieldOffset(10)]
    /* 0x0A */ public int unknownId;
    [FieldOffset(14)]
    /* 0x0E */ public int unknown3;
    [FieldOffset(18)]
    /* 0x12 */ public int unknown4;
    [FieldOffset(22)]
    /* 0x16 */ public int unknown5;
    [FieldOffset(26)]
    /* 0x1A */ public byte unknownbyte14;
    [FieldOffset(27)]
    /* 0x1B */ public byte unknownbyte15;
    [FieldOffset(28)]
    /* 0x1C */ public fixed byte AF[6]; // Real48
    [FieldOffset(34)]
    /* 0x22 */ public int unknown8;
    [FieldOffset(38)]
    /* 0x26 */ public int unknown9;
    [FieldOffset(42)]
    /* 0x2A */ public int unknown10;
    [FieldOffset(46)]
    /* 0x2E */ public int unknown11;
    [FieldOffset(50)]
    /* 0x32 */ public int unknown12;
    [FieldOffset(54)]
    /* 0x36 */ public byte unknownbyte16;
    [FieldOffset(55)]
    /* 0x37 */ public byte unknownbyte17;
    [FieldOffset(56)]
    /* 0x38 */ public byte unknownbyte18;
    [FieldOffset(57)]
    /* 0x39 */ public int commentId;
    [FieldOffset(61)]
    /* 0x3D */ public int templateId;
    [FieldOffset(65)]
    /* 0x41 */ public int conceptId;
    [FieldOffset(69)]
    /* 0x45 */ public byte unknownbyte19;
    [FieldOffset(70)]
    /* 0x46 */ public int unknown17;
    [FieldOffset(74)]
    /* 0x4A */ public int unknown18;
    [FieldOffset(78)]
    /* 0x4E */ public int unknown19;
    [FieldOffset(82)]
    /* 0x52 */ public int unknown20;
    [FieldOffset(86)]
    /* 0x56 */ public int unknown21;
    [FieldOffset(90)]
    /* 0x5A */ public int unknown22;
    [FieldOffset(94)]
    /* 0x5E */ public int unknown23;
    [FieldOffset(98)]
    /* 0x62 */ public int unknown24;
    [FieldOffset(102)]
    /* 0x66 */ public byte unknownbyte20;
    [FieldOffset(103)]
    /* 0x67 */ public fixed byte ordinal[5];
    [FieldOffset(108)]
    /* 0x6C */ public byte unknownbyte21;
    [FieldOffset(109)]
    /* 0x6D */ public byte unknownbyte22;
    [FieldOffset(110)]
    /* 0x6E */ public int ElementNumber;
    [FieldOffset(114)]
    /* 0x72 */ public int unknown28;

    public static readonly int SizeOfElementsElem = Marshal.SizeOf(typeof(InfElementsElem17));
  }
}
