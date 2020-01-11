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
// Created On:   2019/08/09 18:26
// Modified On:  2019/12/14 20:01
// Modified By:  Alexis

#endregion




using System;
using Process.NET.Patterns;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.SuperMemo.SuperMemo17
{
  public partial class SM17Natives
  {
    public class TSMMain17 : TSMMain
    {
      #region Constructors

      public TSMMain17()
      {
        InstancePtr = new IntPtr(0x00BBFE80);

        SelectDefaultConceptCallSig = new DwordCallPattern(
          "E8 ? ? ? ? B1 01 8B 55 F8 8B 45 FC E8 ? ? ? ? 33 C0",
          1);
      }

      #endregion




      #region Properties Impl - Public

      public override IntPtr InstancePtr { get; }

      // TSMMain.SelectDefaultConcept
      public override IMemoryPattern SelectDefaultConceptCallSig { get; }

      #endregion
    }
  }
}
