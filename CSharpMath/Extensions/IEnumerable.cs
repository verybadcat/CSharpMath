using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpMath {
  public static partial class Extensions {
    public static bool IsEmpty<T>(this IEnumerable<T> enumerable) {
      if (enumerable == null) return true;
      foreach (var _ in enumerable) return false;
      return true;
    }
    public static bool IsNonempty<T>(this IEnumerable<T> enumerable) => !enumerable.IsEmpty();
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
        using var enum1 = first.GetEnumerator();
        using var enum2 = second.GetEnumerator();
        using var enum3 = third.GetEnumerator();
        while (enum1.MoveNext() && enum2.MoveNext() && enum3.MoveNext()) {
          yield return resultSelector(
              enum1.Current,
              enum2.Current,
              enum3.Current);
        }
      }
    }
    public static bool SequenceEqual<T>(this IEnumerable<T> enumerable,
      IEnumerable<T> otherEnumerable, Func<T, T, bool>? equalityTester = null) {
      if (equalityTester == null) equalityTester = EqualityComparer<T>.Default.Equals;
      bool r;
      if (enumerable == null && otherEnumerable == null) {
        r = true;
      } else if (enumerable != null && otherEnumerable != null) {
        if (r = enumerable.Count() == otherEnumerable.Count()) {
          using var enum1 = enumerable.GetEnumerator();
          using var enum2 = otherEnumerable.GetEnumerator();
          while (r && enum1.MoveNext() && enum2.MoveNext()) {
            r = equalityTester(enum1.Current, enum2.Current);
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