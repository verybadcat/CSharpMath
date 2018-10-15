using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath {
  public static class IEnumerableExtensions {
    public static bool IsEmpty<T>(this IEnumerable<T> enumerable) {
      if (enumerable == null) return true;
      foreach (T obj in enumerable) return false;
      return true;
    }
    public static bool IsNonempty<T>(this IEnumerable<T> enumerable) => !enumerable.IsEmpty();

    public static void CopyTo<T>(this IEnumerable<T> enumerable, T[] array, int index = 0) {
      foreach (var t in enumerable) array[index++] = t;
    }

    public static IEnumerable<TResult> Zip<TFirst, TSecond, TThird, TResult>(
      this IEnumerable<TFirst> first,
      IEnumerable<TSecond> second,
      IEnumerable<TThird> third,
      Func<TFirst, TSecond, TThird, TResult> resultSelector) {
      if (first is null)
        throw new ArgumentNullException(nameof(first), "first is null.");
      if (second is null)
        throw new ArgumentNullException(nameof(second), "second is null.");
      if (third is null)
        throw new ArgumentNullException(nameof(third), "third is null.");
      return ZipIterator();
      IEnumerable<TResult> ZipIterator() {
        using (var enum1 = first.GetEnumerator())
        using (var enum2 = second.GetEnumerator())
        using (var enum3 = third.GetEnumerator()) {
          while (enum1.MoveNext() && enum2.MoveNext() && enum3.MoveNext()) {
            yield return resultSelector(
                enum1.Current,
                enum2.Current,
                enum3.Current);
          }
        }
      }
    }

    public static bool EqualsEnumerable<T>(this IEnumerable<T> enumerable,
      IEnumerable<T> otherEnumerable, Func<T, T, bool> equalityTester = null) {
      if (equalityTester == null) equalityTester = EqualityComparer<T>.Default.Equals;
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