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
// Created On:   2019/02/13 13:55
// Modified On:  2019/02/21 22:56
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SuperMemoAssistant.Extensions
{
  public static class DictionaryEx
  {
    #region Methods

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

    public static T SafeGet<TKey, T>(this ConcurrentDictionary<TKey, T> dic,
                                     TKey                               key,
                                     T                                  defaultRet = default)
    {
      if (dic.TryGetValue(key, out T ret))
        return ret;

      return defaultRet;
    }

    public static T SafeGet<TKey, T>(this Dictionary<TKey, T> dic,
                                     TKey                     key,
                                     T                        defaultRet = default)
    {
      if (dic.ContainsKey(key))
        return dic[key];

      return defaultRet;
    }

    public static T SafeGet<TKey, T>(this IReadOnlyDictionary<TKey, T> dic,
                                     TKey                              key,
                                     T                                 defaultRet = default)
    {
      if (dic.ContainsKey(key))
        return dic[key];

      return defaultRet;
    }

    //public static T SafeGet<TKey, T>(this IDictionary<TKey, T> dic,
    //                                 TKey                      key,
    //                                 T                         defaultRet = default)
    //{
    //  if (dic.ContainsKey(key))
    //    return dic[key];

    //  return defaultRet;
    //}

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
