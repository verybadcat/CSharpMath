using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath
{
    public static class IEnumerableExtensions
    {
    public static bool IsEmpty<T>(this IEnumerable<T> enumerable) {
      if (enumerable == null) {
        return true;
      }
      foreach (T obj in enumerable) {
        return false;
      }
      return true;
    }
    public static bool IsNonempty<T>(this IEnumerable<T> enumerable)
      => !enumerable.IsEmpty();
    public static bool EqualsEnumerable<T>(this IEnumerable<T> enumerable, IEnumerable<T> otherEnumerable,
  Func<T, T, bool> equalityTester = null) {
      if (equalityTester == null) {
        equalityTester = (T t1, T t2) => t1.Equals(t2);
      }
      bool r;
      if (enumerable == null && otherEnumerable == null) {
        r = true;
      } else if (enumerable != null && otherEnumerable != null) {
        r = enumerable.Count() == otherEnumerable.Count();
        if (r) {
          using (IEnumerator<T> enum1 = enumerable.GetEnumerator())
          using (IEnumerator<T> enum2 = otherEnumerable.GetEnumerator()) {
            while (r && enum1.MoveNext() && enum2.MoveNext()) {
              T curr1 = enum1.Current;
              T curr2 = enum2.Current;
              r = equalityTester(curr1, curr2);
            }
          }
        }
      } else {
        // one enumerable is null and the other isn't
        r = false;
      }
      return r;
    }
  }
}
