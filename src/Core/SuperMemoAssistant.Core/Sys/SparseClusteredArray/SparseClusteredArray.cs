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
// Modified On:  2020/03/05 18:49
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SuperMemoAssistant.Extensions;

namespace SuperMemoAssistant.Sys.SparseClusteredArray
{
  public partial class SparseClusteredArray<T>
  {
    #region Properties & Fields - Non-Public

    internal ReaderWriterLockSlim Lock     { get; } = new ReaderWriterLockSlim();
    internal List<Segment>        Segments { get; } = new List<Segment>();

    #endregion




    #region Methods

    public void Clear()
    {
      Lock.EnterWriteLock();

      try
      {
        Segments.Clear();
      }
      finally
      {
        Lock.ExitWriteLock();
      }
    }

    // TODO: Remove(Bounds bounds)

    public void Write(T[]    data,
                      int    position,
                      Bounds dataSegment)
    {
      if (dataSegment.Lower < 0)
        throw new ArgumentException("Lower bound cannot be less than 0");

      if (dataSegment.Lower >= data.Length)
        throw new ArgumentException("Lower bound cannot be greater or equal to data's length");

      if (dataSegment.Upper < 0)
        throw new ArgumentException("Upper bound cannot be less than 0");

      if (dataSegment.Upper >= data.Length)
        throw new ArgumentException("Lower bound cannot be greater or equal to data's length");

      if (dataSegment.Length() == data.Length)
      {
        Write(data,
              position);
      }

      else
      {
        T[] newData = new T[dataSegment.Length()];
        Array.Copy(data,
                   dataSegment.Lower,
                   newData,
                   0,
                   newData.Length);

        Write(newData,
              position);
      }
    }

    public void Write(T[] data,
                      int position)
    {
      Bounds bounds = new Bounds(position,
                                 position + data.Length - 1);
      var newSegment = new Segment(data,
                                   position);
      List<(int idx, Segment segment)> oSegs;

      Lock.EnterUpgradeableReadLock();

      try
      {
        oSegs = FindOverlappingAndContiguousSegments(Segments,
                                                     bounds);

        if (oSegs[0].idx < 0)
        {
          int complement = ~oSegs[0].idx;

          Lock.EnterWriteLock();

          try
          {
            Segments.Insert(complement,
                            newSegment);
          }
          finally
          {
            Lock.ExitWriteLock();
          }

          return;
        }

        try
        {
          foreach (var seg in oSegs)
            seg.segment.Lock.EnterWriteLock();

          foreach (var seg in oSegs)
            newSegment.Coalesce(seg.segment);

          Lock.EnterWriteLock();

          try
          {
            Segments[oSegs[0].idx] = newSegment;

            if (oSegs.Count > 1)
              Segments.RemoveRange(oSegs[1].idx,
                                   oSegs.Count - 1);
          }
          finally
          {
            Lock.ExitWriteLock();
          }
        }
        finally
        {
          foreach (var seg in oSegs)
            seg.segment.Lock.ExitWriteLock();
        }
      }
      finally
      {
        Lock.ExitUpgradeableReadLock();
      }
    }

    internal (int, List<Segment>) AcquireReadLock(Func<List<Segment>, (int, List<Segment>)> filterFunc)
    {
      List<Segment> localSegs = new List<Segment>(Segments.Count + 2);

      do
      {
        Lock.EnterReadLock();

        try
        {
          localSegs.AddRange(Segments);
        }
        finally
        {
          Lock.ExitReadLock();
        }

        int idxArr = 0;

        // Wrap in try/finally ?
        for (; idxArr < localSegs.Count; idxArr++)
        {
          Segment seg = localSegs[idxArr];

          seg.Lock.EnterReadLock();

          if (seg.Inconsistent)
            break;
        }

        if (idxArr < localSegs.Count)
        {
          for (idxArr = idxArr - 1; idxArr >= 0; idxArr--)
            localSegs[idxArr].Lock.ExitReadLock();

          localSegs.Clear();

          Thread.Yield();
          continue;
        }
        // !Wrap in try/finally ?


        (int ret, List<Segment> filteredSegs) = filterFunc(localSegs);

        foreach (var seg in localSegs.Except(filteredSegs))
          seg.Lock.ExitReadLock();

        return (ret, filteredSegs);
      } while (true);
    }

    internal static (int idx, Segment segment) FindSegment(List<Segment> localSegs,
                                                           int           position)
    {
      int idx = localSegs.BinarySearch(
        new Segment(null,
                    position),
        new PositionalBoundsComparer()
      );

      return (idx, idx >= 0 ? localSegs[idx] : null);
    }

    internal static List<(int idx, Segment segment)> FindOverlappingAndContiguousSegments(List<Segment> localSegs,
                                                                                          IBounds       bounds)
    {
      RelativePosition[] inBoundsPositions = new[]
      {
        RelativePosition.Super,
        RelativePosition.Within,
        RelativePosition.AfterOverlap,
        RelativePosition.AfterContiguous
      };
      List<(int, Segment)> oSegs = new List<(int, Segment)>();
      (int firstIdx, Segment segment) = FindSegment(localSegs,
                                                    Math.Max(0,
                                                             bounds.Lower - 1));

      int itIdx = firstIdx;

      // Equivalent to skipping after the last BeforeNoOverlap
      if (itIdx < 0)
        itIdx = ~itIdx - 1;

      // Equivalent to including the (only) BeforeContiguous or BeforeOverlap or Super
      else
        oSegs.Add((itIdx, segment));

      while (++itIdx < localSegs.Count
        && bounds.PositionOf(localSegs[itIdx]).ContainedIn(inBoundsPositions))
        oSegs.Add((itIdx, localSegs[itIdx]));

      if (oSegs.Count == 0)
      {
        oSegs.Add((firstIdx, null));

        return oSegs;
      }

      return oSegs;
    }

    internal static (int idx, Segment segment) FindSuperSegment(List<Segment> localSegs,
                                                                IBounds       bounds)
    {
      (int idx, Segment segment) = FindSegment(localSegs,
                                               bounds.Lower);

      if (idx < 0)
        return (-1, null);

      var relPos = bounds.PositionOf(segment);

      switch (relPos)
      {
        // Bounds:      |*******|
        // Segment:   |-----------|
        case RelativePosition.Super:
          return (idx, segment);

        // Bounds:          §**-----|
        // Segment:   |-------|
        case RelativePosition.BeforeOverlap:
          return (-1, null);

        case RelativePosition.BeforeContiguous:
        case RelativePosition.AfterContiguous:
        case RelativePosition.BeforeNoOverlap:
        case RelativePosition.AfterNoOverlap:
        case RelativePosition.AfterOverlap:
        case RelativePosition.Within:
        default:
          throw new InvalidOperationException($"Invalid RelativePosition {relPos}, this shouldn't happen");
      }
    }

    #endregion
  }
}
