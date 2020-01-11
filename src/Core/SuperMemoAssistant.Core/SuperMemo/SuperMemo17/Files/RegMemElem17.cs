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
// Created On:   2018/05/12 18:47
// Modified On:  2018/12/10 13:10
// Modified By:  Alexis

#endregion




using System.Runtime.InteropServices;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Files
{
  /// <summary>
  ///   information about all registry members stored in the given registry, e.g.: - number
  ///   of using elements, - pointer to the *.rtx file storing the name of the text registry member,
  ///   - pointer to the *.lst file storing the list of elements using a given member, - etc.
  /// </summary>
  [StructLayout(LayoutKind.Explicit,
    Pack = 1,
    Size = 30)]
  public class RegMemElem17
  {
    [FieldOffset(0)]  public int  useCount;
    [FieldOffset(4)]  public byte linkType;
    [FieldOffset(5)]  public byte unknown1;
    [FieldOffset(6)]  public int  unknown2;
    [FieldOffset(10)] public int  rtxOffset;
    [FieldOffset(14)] public int  rtxLength;
    [FieldOffset(18)] public int  lstRelated; // Physical position ?
    [FieldOffset(22)] public int  slotIdOrOffset; // Id: File ; Position: Rtf, Template (component), ...
    [FieldOffset(26)] public int  slotLengthOrConceptGroup; // Rtf, Template (component), ...
    
    public static int SizeOfMemElem { get; } = Marshal.SizeOf(typeof(RegMemElem17));
  }
}
