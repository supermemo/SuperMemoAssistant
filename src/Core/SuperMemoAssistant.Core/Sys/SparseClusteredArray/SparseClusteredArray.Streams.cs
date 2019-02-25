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
// Created On:   2019/01/20 08:05
// Modified On:  2019/01/20 08:17
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using Segment = SuperMemoAssistant.Sys.SparseClusteredArray.SparseClusteredArray<byte>.Segment;

namespace SuperMemoAssistant.Sys.SparseClusteredArray
{
  public static class SparseClusteredArray
  {
    #region Methods

    public static IEnumerable<SegmentStream> GetStreams(this SparseClusteredArray<byte> sca)
    {
      sca.Lock.EnterReadLock();

      try
      {
        foreach (Segment seg in sca.Segments)
          using (var stream = new SegmentStream(seg))
            yield return stream;
      }
      finally
      {
        sca.Lock.ExitReadLock();
      }
    }

    public static SegmentStream GetSubsetStream(
      this SparseClusteredArray<byte>    sca,
      SparseClusteredArray<byte>.IBounds bounds)
    {
      (int idx, List<Segment> localSegs) = sca.AcquireReadLock((allSegs) =>
        {
          (int superIdx, Segment superSeg) = SparseClusteredArray<byte>.FindSuperSegment(allSegs, bounds);
          return (superIdx, new List<Segment>() { superSeg });
        }
      );

      return idx < 0
        ? null
        : new SegmentStream(localSegs[0], false);
    }

    #endregion
  }

  public class SegmentStream : Stream
  {
    #region Properties & Fields - Non-Public

    protected int     _position;
    protected Segment Segment { get; set; }

    #endregion




    #region Constructors

    internal SegmentStream(Segment segment,
                           bool    shouldLock = true)
    {
      Segment  = segment;
      Position = Segment.Position;

      if (shouldLock)
        Segment.Lock.EnterReadLock();
    }

    protected override void Dispose(bool disposing)
    {
      Segment.Lock.ExitReadLock();

      base.Dispose(disposing);
    }

    #endregion




    #region Properties & Fields - Public

    public int Lower => Segment.Lower;
    public int Upper => Segment.Upper;

    public int RelativePosition => _position - Lower;

    #endregion




    #region Properties Impl - Public

    public override bool CanRead  => true;
    public override bool CanSeek  => true;
    public override bool CanWrite => false;
    public override long Length   => Segment.Length;
    public override long Position
    {
      get { return _position; }
      set
      {
        if (value > Segment.Position + Length)
          throw new ArgumentException("Position cannot be greater than end of Segment");

        if (value < Segment.Lower)
          throw new ArgumentException("Position cannot be lower than beginning of Segment");

        _position = (int)value;
      }
    }

    #endregion




    #region Methods Impl

    public override void Flush()
    {
      throw new NotImplementedException();
    }

    public override int Read(byte[] buffer,
                             int    offset,
                             int    count)
    {
      int readLength = Math.Min((int)(Upper + 1 - Position), count);

      if (readLength <= 0)
        return 0;

      int from = (int)Position;
      int to   = from + readLength - 1;

      var segIter = Segment.Get2DEnumerator(from, to);

      while (segIter.MoveNext())
      {
        var sSeg = segIter.Current;

        for (int i = sSeg._fromIdx; i <= sSeg._toIdx; i++, offset++)
          buffer[offset] = sSeg._arr[i];
      }

      Position += readLength;

      return readLength;
    }

    public override long Seek(long       offset,
                              SeekOrigin origin)
    {
      switch (origin)
      {
        case SeekOrigin.Begin:
          Position = offset;
          break;

        case SeekOrigin.Current:
          Position += offset;
          break;

        case SeekOrigin.End:
          Position = Upper + 1 - offset;
          break;
      }

      return Position;
    }

    public override void SetLength(long value)
    {
      throw new NotImplementedException();
    }

    public override void Write(byte[] buffer,
                               int    offset,
                               int    count)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
