using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperMemoAssistant.Extensions
{
  public static class ObjectEx
  {
    public static T With<T>(this T obj, Action<T> withAction)
    {
      withAction(obj);

      return obj;
    }

    public static bool ContainedIn<T1, T2>(this T1 obj, IEnumerable<T2> col)
      where T2 : T1
    {
      return col.Any(e => e?.Equals(obj) ?? false);
    }
  }
}
