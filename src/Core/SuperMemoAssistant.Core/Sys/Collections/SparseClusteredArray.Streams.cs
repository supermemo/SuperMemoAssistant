using System;
using System.Collections.Generic;
using System.IO;
using static SuperMemoAssistant.Sys.Collections.SparseClusteredArray<byte>;
using Segment = SuperMemoAssistant.Sys.Collections.SparseClusteredArray<byte>.Segment;

namespace SuperMemoAssistant.Sys.Collections
{
  public static class SparseClusteredArray
  {
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
      this SparseClusteredArray<byte> sca,
      IBounds bounds)
    {
      (int idx, List<Segment> localSegs) = sca.AcquireReadLock((allSegs) =>
        {
          (int superIdx, Segment superSeg) = FindSuperSegment(allSegs, bounds);
          return (superIdx, new List<Segment>() { superSeg });
        }
      );

      return (idx < 0)
        ? null
        : new SegmentStream(localSegs[0], false);
    }
  }

  public class SegmentStream : Stream
  {
    protected Segment Segment { get; set; }

    internal SegmentStream(Segment segment, bool shouldLock = true)
    {
      Segment = segment;
      Position = Segment.Position;

      if (shouldLock)
        Segment.Lock.EnterReadLock();
    }

    protected override void Dispose(bool disposing)
    {
      Segment.Lock.ExitReadLock();

      base.Dispose(disposing);
    }

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => Segment.Length;
    public int Lower => Segment.Lower;
    public int Upper => Segment.Upper;

    protected int _position;
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

    public int RelativePosition => _position - Lower;

    public override void Flush()
    {
      throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      int readLength = Math.Min((int)(Upper + 1 - Position), count);

      if (readLength <= 0)
        return 0;

      int from = (int)Position;
      int to = from + readLength - 1;

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

    public override long Seek(long offset, SeekOrigin origin)
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

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotImplementedException();
    }
  }
}
