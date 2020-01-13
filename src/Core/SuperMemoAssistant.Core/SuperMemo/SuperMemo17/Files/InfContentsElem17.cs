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
// Created On:   2018/05/19 14:01
// Modified On:  2018/12/10 13:09
// Modified By:  Alexis

#endregion




using System.Runtime.InteropServices;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Files
{
  [StructLayout(LayoutKind.Explicit,
    Pack = 1,
    Size = 37)]
  public class InfContentsElem17
  {
    [FieldOffset(0)]
    /* 0x00 */ public byte deleted;
    [FieldOffset(1)]
    /* 0x01 */ public int parentId;
    [FieldOffset(5)]
    /* 0x05 */ public int firstChildId;
    [FieldOffset(9)]
    /* 0x09 */ public int lastChildId; //Unsure
    [FieldOffset(13)]
    /* 0x0D */ public int prevSiblingId;
    [FieldOffset(17)]
    /* 0x11 */ public int nextSiblingId;
    [FieldOffset(21)]
    /* 0x15 */ public int descendantCount;
    [FieldOffset(25)]
    /* 0x19 */ public int childrenCount;
    [FieldOffset(29)]
    /* 0x1D */ public int unknown9;
    [FieldOffset(33)]
    /* 0x21 */ public int unknown10;

    public static readonly int SizeOfContentsElem = Marshal.SizeOf(typeof(InfContentsElem17));
  }
}
