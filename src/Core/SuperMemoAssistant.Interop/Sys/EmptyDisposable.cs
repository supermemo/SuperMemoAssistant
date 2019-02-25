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
// Created On:   2019/01/20 08:36
// Modified On:  2019/01/20 08:36
// Modified By:  Alexis

#endregion




using System;

namespace SuperMemoAssistant.Sys
{
  /// <summary>
  ///   A disposable class that does nothing. Original from: https://github.com/Wyamio/Wyam/
  ///   Copyright (c) 2014 Dave Glick
  /// </summary>
  public class EmptyDisposable : IDisposable
  {
    #region Constants & Statics

#pragma warning disable SA1401 // Fields must be private
    /// <summary>A singleton instance of the <see cref="EmptyDisposable" />.</summary>
    public static readonly EmptyDisposable Instance = new EmptyDisposable();
#pragma warning restore SA1401 // Fields must be private

    #endregion




    #region Constructors

    /// <summary>Does nothing.</summary>
    public void Dispose()
    {
      // Do nothing
    }

    #endregion
  }
}
