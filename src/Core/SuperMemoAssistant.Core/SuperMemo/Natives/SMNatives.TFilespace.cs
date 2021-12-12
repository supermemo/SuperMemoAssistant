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

#endregion




// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

namespace SuperMemoAssistant.SuperMemo.Natives
{
  using System;
  using Extensions;
  using Process.NET.Execution;
  using Process.NET.Memory;
  using Process.NET.Native.Types;
  using Process.NET.Patterns;

  public partial class SMNatives
  {
    public class TFileSpace
    {
      #region Constructors

      public TFileSpace(TDatabase db, NativeData nativeData)
      {
        InstancePtr = new ObjPtr(db.InstancePtr, nativeData.Pointers[NativePointer.Registry_FileSpaceInstance]);

        EmptySlotsPtr     = new ObjPtr(InstancePtr, nativeData.Pointers[NativePointer.FileSpace_EmptySlotsOffset]);
        AllocatedSlotsPtr = new ObjPtr(InstancePtr, nativeData.Pointers[NativePointer.FileSpace_AllocatedSlotsOffset]);

        GetTopSlotCallSig     = nativeData.GetMemoryPattern(NativeMethod.FileSpace_GetTopSlot);
        IsSlotOccupiedCallSig = nativeData.GetMemoryPattern(NativeMethod.FileSpace_IsSlotOccupied);

        GetTopSlot = new Procedure<Func<IntPtr, bool, int>>(
          "GetTopSlot",
          CallingConventions.Register,
          GetTopSlotCallSig
        );
        IsSlotOccupied = new Procedure<Func<IntPtr, int, bool>>(
          "IsSlotOccupied",
          CallingConventions.Register,
          IsSlotOccupiedCallSig
        );
      }

      #endregion




      #region Properties & Fields - Public

      public ObjPtr InstancePtr { get; }

      public ObjPtr EmptySlotsPtr     { get; }
      public ObjPtr AllocatedSlotsPtr { get; }


      // TFileSpace.GetTopSlot
      public IMemoryPattern GetTopSlotCallSig { get; }
      // TFileSpace.IsSlotOccupied
      public IMemoryPattern IsSlotOccupiedCallSig { get; }

      public Procedure<Func<IntPtr, bool, int>> GetTopSlot     { get; }
      public Procedure<Func<IntPtr, int, bool>> IsSlotOccupied { get; }

      #endregion
    }
  }
}
