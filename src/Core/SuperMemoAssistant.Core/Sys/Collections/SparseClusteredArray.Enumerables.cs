using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Sys.Collections
{
  public class SparseClusteredArrayEnumerable<TEnum> : IEnumerable<TEnum>
  {
    internal SparseClusteredArrayEnumerable(Func<IEnumerator<TEnum>> enumFunc)
    {
      EnumFunc = enumFunc;
    }

    public Func<IEnumerator<TEnum>> EnumFunc { get; }

    public IEnumerator<TEnum> GetEnumerator()
    {
      return EnumFunc();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  partial class SparseClusteredArray<T>
    : IEnumerable<(T[] _arr, int _fromIdx, int _toIdx, int _itIdx, int _absIdx)>
  {
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<(T[] _arr, int _fromIdx, int _toIdx, int _itIdx, int _absIdx)> GetEnumerator()
    {
      List<Segment> localSegs = null;

      try
      {
        (_, localSegs) = AcquireReadLock((allSegs) => (0, allSegs));

        for (int idxArr = 0; idxArr < localSegs.Count; idxArr++)
        {
          var seg = localSegs[idxArr];
          var segIter = seg.Get2DEnumerator(seg.Lower, seg.Upper);

          while (segIter.MoveNext())
            yield return segIter.Current;
        }
      }
      finally
      {
        for (int idxArr = 0; idxArr < localSegs?.Count; idxArr++)
          localSegs[idxArr].Lock.ExitReadLock();

        localSegs?.Clear();
      }
    }

    //public IEnumerator<T> Read1D(IBounds bounds)
    //{
    //  Segment segment = null;
    //  int idx;

    //  Lock.EnterReadLock();
    //  /////// Exit
    //  do
    //  {
    //    (idx, segment) = FindSuperSegment(bounds);

    //    if (idx < 0)
    //      return null;

    //    segment.Lock.EnterReadLock();

    //    if (segment.Inconsistent)
    //    {
    //      segment.Lock.ExitReadLock();
    //      segment = null;
    //      Thread.Yield();
    //      continue;
    //    }

    //    return new Iterator1D(segment.Lock, segment.Get1DEnumerator(bounds.Lower, bounds.Upper));
    //  } while (true);
    //}

    public IEnumerable<(T[] _arr, int _fromIdx, int _toIdx, int _itIdx, int _absIdx)>
      Subset(IBounds bounds)
    {
      return new SparseClusteredArrayEnumerable<(T[] _arr, int _fromIdx, int _toIdx, int _itIdx, int _absIdx)>(() =>
        {
          (int idx, List<Segment> localSegs) = AcquireReadLock((allSegs) =>
            {
              (int superIdx, Segment superSeg) = FindSuperSegment(allSegs, bounds);
              return (superIdx, new List<Segment>() { superSeg });
            }
          );

          return (idx < 0)
            ? null
            : new Iterator2D(localSegs[0].Lock, localSegs[0].Get2DEnumerator(bounds.Lower, bounds.Upper));
        }
      );
    }
  }
}
