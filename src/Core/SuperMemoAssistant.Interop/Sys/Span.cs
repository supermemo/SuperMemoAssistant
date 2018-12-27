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
// Created On:   2018/12/24 02:02
// Modified On:  2018/12/25 22:31
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;

// ReSharper disable ArrangeRedundantParentheses

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags

namespace SuperMemoAssistant.Sys
{
  public class Span
  {
    #region Constructors

    public Span()
    {
      StartIdx = EndIdx = -1;
    }

    public Span(Span other)
    {
      StartIdx = other.StartIdx;
      EndIdx   = other.EndIdx;
    }

    public Span(int startIdx,
                int endIdx)
    {
      StartIdx = startIdx;
      EndIdx   = endIdx;
    }

    #endregion




    #region Properties & Fields - Public

    public int StartIdx { get; }
    public int EndIdx   { get; }
    public int Length   => EndIdx - StartIdx + 1;

    #endregion




    #region Methods Impl

    public override bool Equals(object obj)
    {
      Span other = obj as Span;

      if (other == null)
        return false;

      return StartIdx == other.StartIdx && EndIdx == other.EndIdx;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      unchecked
      {
        return (StartIdx * 397) ^ EndIdx;
      }
    }

    #endregion




    #region Methods

    public bool Adjacent(Span other)
    {
      return StartIdx == other.EndIdx + 1 || EndIdx == other.StartIdx - 1;
    }

    public bool Overlaps(Span     other,
                         out Span overlap)
    {
      if (StartIdx >= other.StartIdx && StartIdx <= other.EndIdx)
      {
        overlap = new Span(StartIdx,
                           Math.Min(EndIdx,
                                    other.EndIdx));
        return true;
      }

      if (EndIdx >= other.StartIdx && EndIdx <= other.EndIdx)
      {
        overlap = new Span(Math.Max(StartIdx,
                                    other.StartIdx),
                           EndIdx);
        return true;
      }

      if (StartIdx <= other.StartIdx && EndIdx >= other.EndIdx)
      {
        overlap = new Span(other);
        return true;
      }

      overlap = null;

      return false;
    }

    public bool IsWithin(int idx) => StartIdx <= idx && EndIdx >= idx;

    protected bool Equals(Span other)
    {
      return ReferenceEquals(other, null) == false
        && StartIdx == other.StartIdx && EndIdx == other.EndIdx;
    }

    public static bool operator ==(Span span1,
                                   Span span2)
    {
      return ReferenceEquals(span1, span2)
        || (span1?.Equals(span2) ?? false);
    }

    public static bool operator !=(Span span1,
                                   Span span2)
    {
      return !(span1 == span2);
    }

    public static Span operator +(Span span,
                                  int  n)
    {
      return new Span(span.StartIdx + n,
                      span.EndIdx + n);
    }

    public static Span operator -(Span span,
                                  int  n)
    {
      return new Span(span.StartIdx - n,
                      span.EndIdx - n);
    }

    public static Span operator +(Span span1,
                                  Span span2)
    {
      if (span1.Adjacent(span2) == false && span1.Overlaps(span2,
                                                           out _) == false)
        throw new ArgumentException("Spans must be adjacent to be merged");

      return new Span(Math.Min(span1.StartIdx,
                               span2.StartIdx),
                      Math.Max(span1.EndIdx,
                               span2.EndIdx));
    }

    #endregion
    
    public class PositionalComparer : Comparer<Span>
    {
      #region Methods Impl

      public override int Compare(Span x,
                                  Span y)
      {
        if (x == null)
          throw new ArgumentNullException(nameof(x));
        if (y == null)
          throw new ArgumentNullException(nameof(y));

        return x.IsWithin(y.StartIdx) || y.IsWithin(x.StartIdx) ? 0 : x.StartIdx - y.StartIdx;
      }

      #endregion
    }
  }
}
