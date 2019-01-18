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
// Created On:   2019/01/16 15:36
// Modified On:  2019/01/16 21:50
// Modified By:  Alexis

#endregion




using System;

// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Layout.Legacy.Grid
{
  public class GridSize
  {

    #region Constructors

    public GridSize(int size,
                    int span = 1)
    {
      if (span < 1)
        throw new ArgumentException($"Span must be at least 1, value is {span}");

      Size = size;
      Span = span;
    }

    #endregion




    #region Properties & Fields - Public

    public int Size { get; }
    public int Span { get; }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null,
                          obj))
        return false;
      if (ReferenceEquals(this,
                          obj))
        return true;
      if (obj.GetType() != GetType())
        return false;

      return Equals((GridSize)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      unchecked
      {
        return (Size * 397) ^ Span;
      }
    }

    #endregion




    #region Methods

    protected bool Equals(GridSize other)
    {
      return Size == other.Size && Span == other.Span;
    }

    public static bool operator ==(GridSize left,
                                   GridSize right)
    {
      return Equals(left,
                    right);
    }

    public static bool operator !=(GridSize left,
                                   GridSize right)
    {
      return !Equals(left,
                     right);
    }

    public static implicit operator int(GridSize size) => size.Size;

    public static implicit operator GridSize(int size) => new GridSize(size);

    #endregion
  }
}
