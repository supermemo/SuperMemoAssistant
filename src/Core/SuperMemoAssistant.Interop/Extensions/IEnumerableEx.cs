using System;
using System.Collections.Generic;

namespace SuperMemoAssistant.Extensions
{
  public static class IEnumerableEx
  {
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> elements, Action<T> action)
    {
      foreach (T elem in elements)
        action(elem);

      return elements;
    }
  }
}
