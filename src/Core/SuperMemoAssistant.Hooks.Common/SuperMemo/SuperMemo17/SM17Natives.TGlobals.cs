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
// Created On:   2019/12/13 20:24
// Modified On:  2019/12/14 19:54
// Modified By:  Alexis

#endregion




using System;

// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.SuperMemo.SuperMemo17
{
  public partial class SM17Natives
  {
    /// <summary>Global variables</summary>
    public class TGlobals17 : TGlobals
    {
      #region Constructors

      /// <inheritdoc />
      public TGlobals17()
      {
        CurrentConceptIdPtr = new IntPtr(0x00BBCDD0);
        CurrentRootIdPtr    = new IntPtr(0x00BBCDD4);
        CurrentHookIdPtr    = new IntPtr(0x00BBCDD8);

        IgnoreUserConfirmationPtr = new IntPtr(0x00BC0007);
      }

      #endregion




      #region Properties Impl - Public

      public override IntPtr CurrentConceptIdPtr { get; }
      public override IntPtr CurrentRootIdPtr    { get; }
      public override IntPtr CurrentHookIdPtr    { get; }

      public override IntPtr IgnoreUserConfirmationPtr { get; }

      #endregion
    }
  }
}
