﻿#region License & Metadata

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
// Created On:   2020/01/13 16:38
// Modified On:  2020/01/13 21:02
// Modified By:  Alexis

#endregion




using System;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SuperMemo.Common;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17
{
  public class SM17
    : SuperMemoCore
  {
    #region Constants & Statics

    public static readonly Version[] Versions =
    {
      new("17.40"),
      new("18.03"),
      new("18.04"),
      new("18.041"),
      new("18.05"),
    };
    public const string RE_WindowTitle = "([^\\(]+) \\(SuperMemo 17: (.+)\\)";

    #endregion




    #region Constructors

    /// <summary>SM17 Management interface</summary>
    /// <param name="collection">Target collection to open</param>
    /// <param name="binPath">SuperMemo bin path</param>
    public SM17(SMCollection collection,
                string       binPath)
      : base(collection, binPath) { }

    #endregion
  }
}
