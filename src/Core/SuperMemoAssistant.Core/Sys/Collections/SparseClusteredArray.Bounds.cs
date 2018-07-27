using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Sys.Collections
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
      public override int Compare(IBounds x, IBounds y)
      {
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
    }

    public interface IBounds
    {
      int Lower { get; }
      int Upper { get; }
    }

    public class Bounds : IBounds
    {
      public Bounds(int lower, int upper)
      {
        if (lower > upper)
          throw new ArgumentException("Lower can't be greater than Upper");

        Lower = lower;
        Upper = upper;
      }

      public int Lower { get; }
      public int Upper { get; }
    }
  }

  public static class IBoundsEx
  {
    public static int Length<T>(
      this SparseClusteredArray<T>.IBounds bounds)
    {
      return bounds.Upper - bounds.Lower + 1;
    }

    public static RelativePosition IsWithin<T>(
      this SparseClusteredArray<T>.IBounds bounds,
      int position)
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
      SparseClusteredArray<T>.IBounds oBounds)
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
      else if ((oBounds.Lower > bounds.Lower && oBounds.Upper <= bounds.Upper)
        || (oBounds.Lower >= bounds.Lower && oBounds.Upper < bounds.Upper))
        return RelativePosition.Within;

      else if (oBounds.Lower > bounds.Lower)
        return RelativePosition.AfterOverlap;

      else if (oBounds.Upper < bounds.Upper)
        return RelativePosition.BeforeOverlap;
      //!Simplify

      return RelativePosition.Super;
    }
  }
}
