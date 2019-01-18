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
// Created On:   2019/01/17 15:40
// Modified On:  2019/01/17 20:42
// Modified By:  Alexis

#endregion




// ReSharper disable InconsistentNaming

using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Layout.Legacy.Grid
{
  public class VectorSize
  {
    #region Constants & Statics

    public const           string StrAuto = "auto";
    public static readonly Regex  RE_Fill = new Regex("([\\d]?)\\*");


    public const int    Auto    = 0;
    public const string Fill1x  = "*";
    public const string Fill2x  = "2*";
    public const string Fill3x  = "3*";
    public const string Fill5x  = "5*";
    public const string Fill10x = "10*";

    #endregion




    #region Constructors

    public VectorSize(int size)
    {
      Size = size;
    }

    public VectorSize([NotNull] string sizeStr)
    {
      if (sizeStr is StrAuto)
      {
        Size = Auto;
        return;
      }

      var match = RE_Fill.Match(sizeStr);

      if (match.Success)
      {
        int n = match.Groups.Count == 2 && string.IsNullOrWhiteSpace(match.Groups[1].Value) == false
          ? int.Parse(match.Groups[1].Value)
          : 1;
        Size = -n;

        return;
      }

      if (int.TryParse(sizeStr, out int size))
      {
        Size = size;
        return;
      }

      throw new ArgumentException($"Invalid size {sizeStr}");
    }

    #endregion




    #region Properties & Fields - Public

    public int Size { get; }

    public bool IsFill => Size < 0;
    public bool IsAuto => Size == 0;

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

      return Equals((VectorSize)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      return Size;
    }

    #endregion




    #region Methods

    protected bool Equals(VectorSize other)
    {
      return Size == other.Size;
    }

    public static bool operator ==(VectorSize left,
                                   VectorSize right)
    {
      return Equals(left,
                    right);
    }

    public static bool operator !=(VectorSize left,
                                   VectorSize right)
    {
      return !Equals(left,
                     right);
    }

    public static implicit operator int(VectorSize size) => size.Size;

    public static implicit operator VectorSize(int size) => new VectorSize(size);

    public static implicit operator VectorSize(string sizeStr) => new VectorSize(sizeStr);

    #endregion
  }
}
