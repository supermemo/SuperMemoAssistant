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
// Created On:   2020/01/11 15:02
// Modified On:  2020/01/11 18:42
// Modified By:  Alexis

#endregion




using System;
using Anotar.Serilog;
using SuperMemoAssistant.SMA;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.SuperMemo.Natives
{
  public partial class SMNatives
  {
    public class TSMMain
    {
      #region Constructors

      public TSMMain(NativeData nativeData)
      {
        InstancePtr = new IntPtr(nativeData.Pointers[NativePointers.SMMain_InstancePtr]);
      }

      #endregion




      #region Properties & Fields - Public

      public IntPtr InstancePtr { get; }

      #endregion




      #region Methods

      // TSMMain.SelectDefaultConcept
      public bool SelectDefaultConcept(IntPtr smMainPtr, int conceptId)
      {
        try
        {
          NativeMethod.TSMMain_SelectDefaultConcept.ExecuteOnMainThread(smMainPtr, conceptId);

          return true;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      #endregion
    }
  }
}
