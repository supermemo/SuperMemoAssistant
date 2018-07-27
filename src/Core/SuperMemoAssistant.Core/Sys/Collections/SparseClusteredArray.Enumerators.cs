using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Sys.Collections
{
  partial class SparseClusteredArray<T>
  {
    public class Iterator1D : IEnumerator<T>
    {
      private ReaderWriterLockSlim Lock { get; set; }
      private IEnumerator<T> InternalEnumerator { get; }

      internal Iterator1D(
        ReaderWriterLockSlim @lock,
        IEnumerator<T> internalEnumerator)
      {
        Lock = @lock;
        InternalEnumerator = internalEnumerator;
      }

      public T Current => InternalEnumerator.Current;

      object IEnumerator.Current => InternalEnumerator.Current;

      public void Dispose()
      {
        Lock.ExitReadLock();
      }

      public bool MoveNext() => InternalEnumerator.MoveNext();

      public void Reset()
      {
        throw new NotImplementedException();
      }
    }

    public class Iterator2D : IEnumerator<(T[] _arr, int _fromIdx, int _toIdx, int _itIdx, int _absIdx)>
    {
      private ReaderWriterLockSlim Lock { get; set; }
      private IEnumerator<(T[] _arr, int _fromIdx, int _toIdx, int _itIdx, int _absIdx)> InternalEnumerator { get; }

      internal Iterator2D(
        ReaderWriterLockSlim @lock,
        IEnumerator<(T[] _arr, int _fromIdx, int _toIdx, int _itIdx, int _absIdx)> internalEnumerator)
      {
        Lock = @lock;
        InternalEnumerator = internalEnumerator;
      }

      public (T[] _arr, int _fromIdx, int _toIdx, int _itIdx, int _absIdx) Current => InternalEnumerator.Current;

      object IEnumerator.Current => InternalEnumerator.Current;

      public void Dispose()
      {
        Lock.ExitReadLock();
      }

      public bool MoveNext() => InternalEnumerator.MoveNext();

      public void Reset()
      {
        throw new NotImplementedException();
      }
    }
  }
}
