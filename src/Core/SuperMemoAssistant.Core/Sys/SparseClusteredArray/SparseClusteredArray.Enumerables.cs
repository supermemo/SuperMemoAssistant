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

namespace SuperMemoAssistant.Sys.SparseClusteredArray
{
  public class SparseClusteredArrayEnumerable<TEnum> : IEnumerable<TEnum>
  {
    #region Constructors

    internal SparseClusteredArrayEnumerable(Func<IEnumerator<TEnum>> enumFunc)
    {
      EnumFunc = enumFunc;
    }

    #endregion




    #region Properties & Fields - Public

    public Func<IEnumerator<TEnum>> EnumFunc { get; }

    #endregion




    #region Methods Impl

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<TEnum> GetEnumerator()
    {
      return EnumFunc();
    }

    #endregion
  }

  partial class SparseClusteredArray<T>
    : IEnumerable<(T[] _arr, int _fromIdx, int _toIdx, int _itIdx, int _absIdx)>
  {
    #region Methods Impl

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
          var segIter = seg.Get2DEnumerator(seg.Lower,
                                            seg.Upper);

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

    #endregion




    #region Methods

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
              (int superIdx, Segment superSeg) = FindSuperSegment(allSegs,
                                                                  bounds);
              return (superIdx, new List<Segment>() { superSeg });
            }
          );

          return idx < 0
            ? null
            : new Iterator2D(localSegs[0].Lock,
                             localSegs[0].Get2DEnumerator(bounds.Lower,
                                                          bounds.Upper));
        }
      );
    }

    #endregion
  }
}
