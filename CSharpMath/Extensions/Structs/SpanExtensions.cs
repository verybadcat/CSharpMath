using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath {
  public static class SpanExtensions {
    public static StringBuilder ToStringBuilder(this ReadOnlySpan<char> span) {
      var sb = new StringBuilder(span.Length);
      for (int i = 0; i < span.Length; i++) sb.Append(span[i]);
      return sb;
    }
    public static bool Is(this ReadOnlySpan<char> span, char c) =>
      span.Length == 1 && span[0] == c;
    public static bool IsNot(this ReadOnlySpan<char> span, char c) =>
      span.Length != 1 || span[0] != c;
    public static bool Is(this ReadOnlySpan<char> span, string s) =>
      span.SequenceEqual(s.AsSpan());
    public static bool IsNot(this ReadOnlySpan<char> span, string s) =>
      span.SequenceEqual(s.AsSpan());
    public static bool TryAccessDictionary<TValue>(this ReadOnlySpan<char> span, IEnumerable<KeyValuePair<string, TValue>> dict, out TValue value) {
      foreach (var p in dict)
        if (span.SequenceEqual(p.Key.AsSpan())) {
          value = p.Value;
          return true;
        }
      value = default;
      return false;
    }
  }
}
