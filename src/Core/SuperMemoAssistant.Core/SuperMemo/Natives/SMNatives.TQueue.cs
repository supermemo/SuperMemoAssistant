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




// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

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
    /// <summary>Delphi's base class TControl</summary>
    public class TQueue
    {
      #region Constructors

      public TQueue(NativeData nativeData)
      {
        SizeOffset = nativeData.Pointers[NativePointer.Queue_SizeOffset];
        
        LastCallSig = nativeData.GetMemoryPattern(NativeMethod.Queue_Last);
        GetItemCallSig = nativeData.GetMemoryPattern(NativeMethod.Queue_GetItem);

        Last = new Procedure<Func<IntPtr, int>>(
          "Last",
          CallingConventions.Register,
          LastCallSig
        );

        GetItem = new Procedure<Func<IntPtr, int, int>>(
          "GetItem",
          CallingConventions.Register,
          GetItemCallSig
        );
      }

      #endregion




      #region Properties & Fields - Public

      public int SizeOffset { get; }

      // TQueue.Last
      public IMemoryPattern LastCallSig { get; }
      public IMemoryPattern GetItemCallSig { get; }
      
      public Procedure<Func<IntPtr, int>> Last { get; }
      public Procedure<Func<IntPtr, int, int>> GetItem { get; }

      #endregion




      #region Methods

      public int GetSize(IntPtr queuePtr, IMemory memory)
      {
        return new ObjPtr(queuePtr, SizeOffset).Read<int>(memory);
      }

      #endregion
    }
  }
}
