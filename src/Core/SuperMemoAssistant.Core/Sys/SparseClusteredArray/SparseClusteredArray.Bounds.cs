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

// ReSharper disable UnusedTypeParameter

namespace SuperMemoAssistant.Sys.SparseClusteredArray
{
  public enum RelativePosition
  {
    BeforeContiguous,
    AfterContiguous,
    BeforeNoOverlap,
    AfterNoOverlap,
    BeforeOverlap,
    AfterOverlap,
    Within,
    Super
  }

  partial class SparseClusteredArray<T>
  {
    public class PositionalBoundsComparer : Comparer<IBounds>
    {
      #region Methods Impl

      public override int Compare(IBounds x,
                                  IBounds y)
      {
        if (x == null)
          throw new ArgumentNullException(nameof(x));
        if (y == null)
          throw new ArgumentNullException(nameof(y));

        switch (x.IsWithin(y.Lower))
        {
          case RelativePosition.Within:
            return 0;

          case RelativePosition.BeforeNoOverlap:
          case RelativePosition.AfterNoOverlap:
            return x.Lower - y.Lower;

          default:
            throw new InvalidOperationException("Invalid RelativePosition");
        }
      }

      #endregion
    }

    public interface IBounds
    {
      int Lower { get; }
      int Upper { get; }
    }

    public class Bounds : IBounds
    {
      #region Constructors

      public Bounds(int lower,
                    int upper)
      {
        if (lower > upper)
          throw new ArgumentException("Lower can't be greater than Upper");

        Lower = lower;
        Upper = upper;
      }

      #endregion




      #region Properties Impl - Public

      public int Lower { get; }
      public int Upper { get; }

      #endregion
    }
  }

  public static class IBoundsEx
  {
    #region Methods

    public static int Length<T>(
      this SparseClusteredArray<T>.IBounds bounds)
    {
      return bounds.Upper - bounds.Lower + 1;
    }

    public static RelativePosition IsWithin<T>(
      this SparseClusteredArray<T>.IBounds bounds,
      int                                  position)
    {
      if (position < bounds.Lower)
        return RelativePosition.BeforeNoOverlap;

      else if (position > bounds.Upper)
        return RelativePosition.AfterNoOverlap;

      else
        return RelativePosition.Within;
    }

    public static RelativePosition PositionOf<T>(
      this SparseClusteredArray<T>.IBounds bounds,
      SparseClusteredArray<T>.IBounds      oBounds)
    {
      //TODO: Simplify with substraction
      if (oBounds.Upper < bounds.Lower)
      {
        if (oBounds.Upper == bounds.Lower - 1)
          return RelativePosition.BeforeContiguous;

        return RelativePosition.BeforeNoOverlap;
      }

      else if (oBounds.Lower > bounds.Upper)
      {
        if (oBounds.Lower == bounds.Upper + 1)
          return RelativePosition.AfterContiguous;

        return RelativePosition.AfterNoOverlap;
      }
      //!Simplify

      //TODO: Simplify with substraction
      else if (oBounds.Lower > bounds.Lower && oBounds.Upper <= bounds.Upper
        || oBounds.Lower >= bounds.Lower && oBounds.Upper < bounds.Upper)
      {
        return RelativePosition.Within;
      }

      else if (oBounds.Lower > bounds.Lower)
      {
        return RelativePosition.AfterOverlap;
      }

      else if (oBounds.Upper < bounds.Upper)
      {
        return RelativePosition.BeforeOverlap;
      }
      //!Simplify

      return RelativePosition.Super;
    }

    #endregion
  }
}
