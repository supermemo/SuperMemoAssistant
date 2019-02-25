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
// Created On:   2018/05/15 23:24
// Modified On:  2018/12/13 12:53
// Modified By:  Alexis

#endregion




using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SuperMemoAssistant.Sys.SparseClusteredArray
{
  partial class SparseClusteredArray<T>
  {
    public class Iterator1D : IEnumerator<T>
    {
      #region Properties & Fields - Non-Public

      private ReaderWriterLockSlim Lock               { get; set; }
      private IEnumerator<T>       InternalEnumerator { get; }

      object IEnumerator.Current => InternalEnumerator.Current;

      #endregion




      #region Constructors

      internal Iterator1D(
        ReaderWriterLockSlim @lock,
        IEnumerator<T>       internalEnumerator)
      {
        Lock               = @lock;
        InternalEnumerator = internalEnumerator;
      }

      public void Dispose()
      {
        Lock.ExitReadLock();
      }

      #endregion




      #region Properties Impl - Public

      public T Current => InternalEnumerator.Current;

      #endregion




      #region Methods Impl

      public bool MoveNext() => InternalEnumerator.MoveNext();

      public void Reset()
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    public class Iterator2D : IEnumerator<(T[] _arr, int _fromIdx, int _toIdx, int _itIdx, int _absIdx)>
    {
      #region Properties & Fields - Non-Public

      private ReaderWriterLockSlim                                                       Lock               { get; set; }
      private IEnumerator<(T[] _arr, int _fromIdx, int _toIdx, int _itIdx, int _absIdx)> InternalEnumerator { get; }

      object IEnumerator.Current => InternalEnumerator.Current;

      #endregion




      #region Constructors

      internal Iterator2D(
        ReaderWriterLockSlim                                                       @lock,
        IEnumerator<(T[] _arr, int _fromIdx, int _toIdx, int _itIdx, int _absIdx)> internalEnumerator)
      {
        Lock               = @lock;
        InternalEnumerator = internalEnumerator;
      }

      public void Dispose()
      {
        Lock.ExitReadLock();
      }

      #endregion




      #region Properties Impl - Public

      public (T[] _arr, int _fromIdx, int _toIdx, int _itIdx, int _absIdx) Current => InternalEnumerator.Current;

      #endregion




      #region Methods Impl

      public bool MoveNext() => InternalEnumerator.MoveNext();

      public void Reset()
      {
        throw new NotImplementedException();
      }

      #endregion
    }
  }
}
