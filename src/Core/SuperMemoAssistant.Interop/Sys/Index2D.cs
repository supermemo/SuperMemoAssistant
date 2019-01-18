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
// Created On:   2019/01/16 21:16
// Modified On:  2019/01/16 21:19
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.Sys
{
  public class Index2D
  {
    #region Constructors

    public Index2D(int x,
                   int y)
    {
      X = x;
      Y = y;
    }

    #endregion




    #region Properties & Fields - Public

    public int X { get; }
    public int Y { get; }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType() != GetType())
        return false;

      return Equals((Index2D)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      unchecked
      {
        return (X * 397) ^ Y;
      }
    }

    /// <inheritdoc />
    public override string ToString()
    {
      return $"{X}, {Y}";
    }

    #endregion




    #region Methods

    protected bool Equals(Index2D other)
    {
      return X == other.X && Y == other.Y;
    }

    public static bool operator ==(Index2D left,
                                   Index2D right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(Index2D left,
                                   Index2D right)
    {
      return !Equals(left, right);
    }

    public static implicit operator Index2D((int x, int y) indexes) => new Index2D(indexes.x, indexes.y);
    public static implicit operator (int x, int y)(Index2D index) => (index.X, index.Y);

    #endregion
  }
}
