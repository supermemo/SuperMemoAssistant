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
// Modified On:  2020/02/02 23:00
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable PossibleMultipleEnumeration

namespace SuperMemoAssistant.Extensions
{
  public static class IEnumerableEx
  {
    #region Methods

    public static bool None<T>(this IEnumerable<T> elements)
    {
      return elements.Any() == false;
    }

    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> elements,
                                            Action<T>           action)
    {
      foreach (T elem in elements)
        action(elem);

      return elements;
    }

    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
      HashSet<TKey> seenKeys = new HashSet<TKey>();

      foreach (TSource element in source)
        if (seenKeys.Add(keySelector(element)))
          yield return element;
    }

    #endregion
  }
}
