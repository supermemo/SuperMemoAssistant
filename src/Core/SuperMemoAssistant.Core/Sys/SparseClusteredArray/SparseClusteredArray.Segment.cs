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
using System.Collections.Generic;
using System.Threading;

namespace SuperMemoAssistant.Sys.SparseClusteredArray
{
  partial class SparseClusteredArray<T>
  {
    public class SubSegment : IBounds
    {
      #region Constructors

      public SubSegment(T[] array,
                        int position)
      {
        Array    = array;
        Position = position;
      }

      #endregion




      #region Properties & Fields - Public

      public T[] Array { get; private set; }

      public int Position { get; private set; }
      public int Length   => Array.Length;

      #endregion




      #region Properties Impl - Public

      public int Lower => Position;
      public int Upper => Position + Length - 1;

      #endregion
    }

    public class Segment
      : IBounds
    {
      internal Segment(T[] array,
                       int position)
      {
        Segments = new List<SubSegment>(new[]
        {
          new SubSegment(array,
                         position)
        });
        Position = position;
        Length   = array?.Length ?? -1;
      }

      public ReaderWriterLockSlim Lock         { get; }              = new ReaderWriterLockSlim();
      public bool                 Inconsistent { get; private set; } = false;

      public List<SubSegment> Segments { get; private set; }

      public int Position { get; private set; }
      public int Length   { get; private set; }
      public int Lower    => Position;
      public int Upper    => Position + Length - 1;

      public IEnumerator<T> Get1DEnumerator(int fromIdx,
                                            int toIdx)
      {
        var arrEnum = Get2DEnumerator(fromIdx,
                                      toIdx);
        T[] arr;

        while (arrEnum.MoveNext())
        {
          (arr, fromIdx, toIdx, _, _) = arrEnum.Current;

          for (; fromIdx <= toIdx; fromIdx++)
            yield return arr[fromIdx];
        }
      }

      public IEnumerator<(T[] _arr, int _fromIdx, int _toIdx, int _itIdx, int _absIdx)> Get2DEnumerator(int fromIdx,
                                                                                                        int toIdx)
      {
        if (toIdx < fromIdx)
          throw new ArgumentException("To cannot be lower or equal to From");

        if (fromIdx < Lower)
          throw new ArgumentException("From cannot be lower than Lower bound");

        if (fromIdx > Upper)
          throw new ArgumentException("From cannot be greater or equal to Upper bound");

        if (toIdx < Lower)
          throw new ArgumentException("To cannot be lower or equal to Upper bound");

        if (toIdx > Upper)
          throw new ArgumentException("To cannot be greater than Upper bound");

        int remaining = toIdx - fromIdx + 1;

        // Fast-forward to first matching array
        int arrIdx = Segments.BinarySearch(
          new SubSegment(null,
                         fromIdx),
          new PositionalBoundsComparer()
        );

        fromIdx -= Segments[arrIdx].Lower;

        do
        {
          SubSegment sSeg = Segments[arrIdx];
          T[]        arr  = sSeg.Array;
          toIdx = Math.Min(arr.Length,
                           remaining + fromIdx) - 1;

          yield return (arr, fromIdx, toIdx, sSeg.Lower - Lower + fromIdx, sSeg.Lower + fromIdx);

          int lastLength = toIdx - fromIdx + 1;
          remaining -= lastLength;

          arrIdx++;
          fromIdx = 0;
        } while (remaining > 0);
      }

#if false
/// <summary>
/// Execute given <paramref name="action"/> on every element between
/// <paramref name="from"/> and <paramref name="to"/>
/// </summary>
/// <param name="from">Absolute Lower bound</param>
/// <param name="to">Absolute Upper bound</param>
/// <param name="action">Action to execute on each element</param>
      internal void Do(int from, int to, Action<int, ref T> action)
      {
        if (to <= from)
          throw new ArgumentException("To cannot be lower or equal to From");

        if (from < Lower)
          throw new ArgumentException("From cannot be lower than Lower bound");

        if (from >= Upper)
          throw new ArgumentException("From cannot be greater or equal to Upper bound");

        if (to <= Lower)
          throw new ArgumentException("To cannot be lower or equal to Upper bound");

        if (to > Upper)
          throw new ArgumentException("To cannot be greater than Upper bound");

        int idxArr = 0;
        int idx = from - Lower;
        int count = to - from;

        while (idx >= Arrays[idxArr].Length)
          idx -= Arrays[idxArr++].Length;

        do
        {
          T[] arr = Arrays[idxArr];
          to = Math.Min(arr.Length, count + idx);
          count -= to - idx;

          for (; idx < to; idx++)
            action(from++, arr[idx]);

          idxArr++;
        } while (count > 0);
      }
#endif

      /// <summary>
      ///   Coalesce both segments. If there is an overlap, the content of the current one will
      ///   be preserved. Always operate on a new segment so it can mark the overlapping ones as
      ///   inconsistent. This is required to avoid a rare thread sync issue where a read-access method
      ///   is pre-empted in-between obtaining a consistent segment and read-locking it.
      /// </summary>
      /// <param name="segment"></param>
      internal void Coalesce(Segment segment)
      {
        RelativePosition relativePosition = this.PositionOf(segment);

        switch (relativePosition)
        {
          case RelativePosition.BeforeOverlap:
            CoalesceLeftOverlap(segment);
            break;

          case RelativePosition.AfterOverlap:
            CoalesceRightOverlap(segment);
            break;

          case RelativePosition.BeforeContiguous:
            CoalesceLeft(segment);
            break;

          case RelativePosition.AfterContiguous:
            CoalesceRight(segment);
            break;

          case RelativePosition.Super:
            CoalesceSuper(segment);
            break;

          case RelativePosition.Within:
            break;

          default:
            throw new ArgumentException("Trying to coalesce non-contiguous, non-overlapping segments");
        }

        segment.Inconsistent = true; // Move to CoalesceSuper ?
      }

      private void CoalesceLeft(Segment segment)
      {
        Segments.InsertRange(0,
                             segment.Segments);
        Position =  segment.Position;
        Length   += segment.Length;
      }

      private void CoalesceRight(Segment segment)
      {
        Segments.AddRange(segment.Segments);
        Length += segment.Length;
      }

      private void CoalesceLeftOverlap(Segment segment)
      {
        List<SubSegment> toAdd  = new List<SubSegment>();
        int              idxArr = 0;
        SubSegment       sSeg   = segment.Segments[0];

        while (sSeg.Upper < Lower)
        {
          toAdd.Add(sSeg);
          sSeg = segment.Segments[++idxArr];
        }

        if (sSeg.Upper == Lower - 1)
        {
          Segments.InsertRange(0,
                               toAdd);
          return;
        }

        // TODO: Check which array is costlier to alter
        int newLength = Lower - sSeg.Lower;

        T[] arr = sSeg.Array;
        Array.Resize(ref arr,
                     newLength); // This is actually a copy

        toAdd.Add(new SubSegment(arr,
                                 sSeg.Lower));
        Segments.InsertRange(0,
                             toAdd);

        Length   += Lower - segment.Lower;
        Position =  segment.Position;
      }

      private void CoalesceRightOverlap(Segment segment)
      {
        int        idxArr = 0;
        SubSegment sSeg   = segment.Segments[0];

        while (sSeg.Upper < Upper)
          sSeg = segment.Segments[++idxArr];

        // TODO: Check which array is costlier to alter
        if (sSeg.Upper != Upper)
        {
          int newLength = sSeg.Upper - Upper;
          int offset    = Upper - sSeg.Lower + 1;

          T[] newArr = new T[newLength];

          Array.Copy(sSeg.Array,
                     offset,
                     newArr,
                     0,
                     newLength);
          Segments.Add(new SubSegment(newArr,
                                      Upper + 1));
        }

        while (++idxArr < segment.Segments.Count)
          Segments.Add(segment.Segments[idxArr]);

        Length += segment.Upper - Upper;
      }

      private void CoalesceSuper(Segment segment)
      {
        var thisEnum = Get1DEnumerator(Lower,
                                       Upper);
        var arrEnum = segment.Get2DEnumerator(Lower,
                                              Upper);
        T[] arr;
        int fromIdx;
        int toIdx;

        while (arrEnum.MoveNext())
        {
          (arr, fromIdx, toIdx, _, _) = arrEnum.Current;

          for (; fromIdx <= toIdx; fromIdx++)
          {
            thisEnum.MoveNext();
            arr[fromIdx] = thisEnum.Current;
          }
        }

        Segments = segment.Segments;
        Position = segment.Position;
        Length   = segment.Length;
      }
    }
  }
}
