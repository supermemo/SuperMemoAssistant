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
// Created On:   2021/12/04 15:01
// Modified On:  2021/12/04 16:04
// Modified By:  Ki

#endregion




using Anotar.Serilog;
using Process.NET.Types;
using SuperMemoAssistant.SMA;
using System;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.SuperMemo.Natives
{
  public partial class SMNatives
  {
    public class TContents
    {
      #region Constructors

      public TContents(NativeData nativeData)
      {
        InstancePtr = new IntPtr(nativeData.Pointers[NativePointer.Contents_InstancePtr]);
      }

      #endregion




      #region Properties & Fields - Public

      public IntPtr InstancePtr { get; }

      #endregion

      public bool FindText(IntPtr contentsPtr, string text)
      {
        throw new NotImplementedException();
        /* This does nothing!
        try
        {
          string str = "Testng";
          return NativeMethod.Contents_FindText.ExecuteOnMainThread(
            contentsPtr,
            str) == 1;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
        */
      }
    }
  }
}
