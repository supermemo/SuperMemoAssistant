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
// Created On:   2020/01/11 15:01
// Modified On:  2020/01/11 16:01
// Modified By:  Alexis

#endregion




using System;
using Process.NET.Memory;

// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.SuperMemo.Natives
{
  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class SMNatives
  {
    /// <summary>Delphi's base class TApplication</summary>
    public class TApplication
    {
      #region Constructors

      public TApplication(NativeData nativeData)
      {
        OnMessageOffset = nativeData.Pointers[NativePointers.Application_OnMessageOffset];

        TApplicationInstanceAddr = new IntPtr(nativeData.Pointers[NativePointers.Application_InstancePtr]);
        TApplicationOnMessagePtr = new ObjPtr(TApplicationInstanceAddr, OnMessageOffset);
      }

      #endregion




      #region Properties & Fields - Public

      public int OnMessageOffset { get; }

      public IntPtr TApplicationInstanceAddr { get; }
      public ObjPtr TApplicationOnMessagePtr { get; }

      #endregion
    }
  }
}
