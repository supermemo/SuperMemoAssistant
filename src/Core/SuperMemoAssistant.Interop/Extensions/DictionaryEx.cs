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
// Modified On:  2020/03/22 16:35
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SuperMemoAssistant.Extensions
{
  /// <summary>Extension methods for <see cref="IDictionary{TKey,TValue}" /> and related</summary>
  public static class DictionaryEx
  {
    #region Methods

    /// <summary>
    ///   Removes <paramref name="key" /> and set <paramref name="value" /> to the removed
    ///   element if available
    /// </summary>
    /// <typeparam name="TKey">Dictionary Key type</typeparam>
    /// <typeparam name="T">Dictionary Value type</typeparam>
    /// <param name="dic">The dictionary</param>
    /// <param name="key">The key</param>
    /// <param name="value">The removed value or default</param>
    /// <returns>Whether an item was removed</returns>
    public static bool Remove<TKey, T>(this IDictionary<TKey, T> dic,
                                       TKey                      key,
                                       out T                     value)
    {
      if (dic.ContainsKey(key))
      {
        value = dic[key];
        return dic.Remove(key);
      }

      value = default;
      return false;
    }

    /// <summary>Safely tries to retrieve the item <paramref name="key" /> from the dictionary.</summary>
    /// <typeparam name="TKey">Dictionary Key type</typeparam>
    /// <typeparam name="T">Dictionary Value type</typeparam>
    /// <param name="dic">The dictionary</param>
    /// <param name="key">The key</param>
    /// <param name="defaultRet">The default value to return if the item doesn't exit</param>
    /// <returns>
    ///   The value associated with <paramref name="key" /> or <paramref name="defaultRet" />
    /// </returns>
    public static T SafeGet<TKey, T>(this IDictionary<TKey, T> dic,
                                     TKey                      key,
                                     T                         defaultRet = default)
    {
      if (dic.TryGetValue(key, out T ret))
        return ret;

      return defaultRet;
    }

    /// <summary>Safely tries to retrieve the item <paramref name="key" /> from the dictionary.</summary>
    /// <typeparam name="TKey">Dictionary Key type</typeparam>
    /// <typeparam name="T">Dictionary Value type</typeparam>
    /// <param name="dic">The dictionary</param>
    /// <param name="key">The key</param>
    /// <param name="defaultRet">The default value to return if the item doesn't exit</param>
    /// <returns>
    ///   The value associated with <paramref name="key" /> or <paramref name="defaultRet" />
    /// </returns>
    public static T SafeRead<TKey, T>(this IReadOnlyDictionary<TKey, T> dic,
                                      TKey                              key,
                                      T                                 defaultRet = default)
    {
      if (dic.ContainsKey(key))
        return dic[key];

      return defaultRet;
    }

    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
      this IEnumerable<KeyValuePair<TKey, TValue>> source)
    {
      return new ConcurrentDictionary<TKey, TValue>(source);
    }

    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
      this IEnumerable<TValue> source,
      Func<TValue, TKey>       keySelector)
    {
      return new ConcurrentDictionary<TKey, TValue>(
        from v in source
        select new KeyValuePair<TKey, TValue>(keySelector(v),
                                              v));
    }

    public static ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TKey, TValue, TElement>(
      this IEnumerable<TValue> source,
      Func<TValue, TKey>       keySelector,
      Func<TValue, TElement>   elementSelector)
    {
      return new ConcurrentDictionary<TKey, TElement>(
        from v in source
        select new KeyValuePair<TKey, TElement>(keySelector(v),
                                                elementSelector(v)));
    }

    #endregion
  }
}
