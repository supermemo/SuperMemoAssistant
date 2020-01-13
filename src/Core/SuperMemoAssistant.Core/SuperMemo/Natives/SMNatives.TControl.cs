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
// Created On:   2019/12/13 20:26
// Modified On:  2019/12/14 19:56
// Modified By:  Alexis

#endregion





// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.SuperMemo.Natives
{
  public partial class SMNatives
  {
    /// <summary>Delphi's base class TControl</summary>
    public class TControl
    {
      #region Constructors

      public TControl(NativeData nativeData)
      {
        ParentOffset     = nativeData.Pointers[NativePointers.Control_ParentOffset];
        WindowProcOffset = nativeData.Pointers[NativePointers.Control_WindowProcOffset];
        HandleOffset     = nativeData.Pointers[NativePointers.Control_HandleOffset];

        LeftOffset   = nativeData.Pointers[NativePointers.Control_LeftOffset];
        TopOffset    = nativeData.Pointers[NativePointers.Control_TopOffset];
        WidthOffset  = nativeData.Pointers[NativePointers.Control_WidthOffset];
        HeightOffset = nativeData.Pointers[NativePointers.Control_HeightOffset];
      }

      #endregion




      #region Properties & Fields - Public

      public int ParentOffset     { get; }
      public int WindowProcOffset { get; }
      public int HandleOffset     { get; } // TWinControl.Handle

      public int LeftOffset   { get; }
      public int TopOffset    { get; }
      public int WidthOffset  { get; }
      public int HeightOffset { get; }

      #endregion
    }
  }
}
