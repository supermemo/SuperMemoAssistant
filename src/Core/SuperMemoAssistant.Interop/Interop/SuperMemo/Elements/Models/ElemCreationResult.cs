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
// Created On:   2019/04/16 14:43
// Modified On:  2019/04/17 00:22
// Modified By:  Alexis

#endregion




using System;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;

namespace SuperMemoAssistant.Interop.SuperMemo.Elements.Models
{
  [Serializable]
  public class ElemCreationResult
  {
    #region Constructors

    public ElemCreationResult(ElemCreationResultCode result, ElementBuilder builder)
    {
      Result  = result;
      Builder = builder;
    }

    #endregion




    #region Properties & Fields - Public

    public ElemCreationResultCode Result    { get; set; }
    public ElementBuilder         Builder   { get; }
    public int                    ElementId { get; set; } = -1;

    public bool Success => Result.HasFlag(ElemCreationResultCode.Success);

    #endregion
  }
}
