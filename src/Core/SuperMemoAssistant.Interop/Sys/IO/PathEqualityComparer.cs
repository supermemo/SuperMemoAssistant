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
// Created On:   2019/01/20 08:10
// Modified On:  2019/01/26 01:16
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;

namespace SuperMemoAssistant.Sys.IO
{
  /// <summary>Compares <see cref="NormalizedPath" /> instances.</summary>
  /// https://github.com/Wyamio/Wyam/ Copyright (c) 2014 Dave Glick
  public sealed class PathEqualityComparer : IEqualityComparer<NormalizedPath>
  {
    #region Methods Impl

    /// <summary>Determines whether the specified <see cref="NormalizedPath" /> instances are equal.</summary>
    /// <param name="x">The first <see cref="NormalizedPath" /> to compare.</param>
    /// <param name="y">The second <see cref="NormalizedPath" /> to compare.</param>
    /// <returns>
    ///   True if the specified <see cref="NormalizedPath" /> instances are equal; otherwise,
    ///   false.
    /// </returns>
    public bool Equals(NormalizedPath x,
                       NormalizedPath y)
    {
      if (x == null && y == null)
        return true;

      if (x == null || y == null)
        return false;

      return x.Equals(y);
    }

    /// <summary>Returns a hash code for the specified <see cref="NormalizedPath" />.</summary>
    /// <param name="obj">The path.</param>
    /// <returns>
    ///   A hash code for this instance, suitable for use in hashing algorithms and data
    ///   structures like a hash table.
    /// </returns>
    public int GetHashCode(NormalizedPath obj)
    {
      if (obj == null)
        throw new ArgumentNullException(nameof(obj));

      return obj.GetHashCode();
    }

    #endregion
  }
}
