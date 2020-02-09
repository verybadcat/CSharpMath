using System;
using System.Linq;

namespace CSharpMath {
  public static class ReadOnlySpanExtensions {
    public static bool Is(this ReadOnlySpan<char> span, char c) =>
      span.Length == 1 && span[0] == c;
    public static bool IsNot(this ReadOnlySpan<char> span, char c) =>
      span.Length != 1 || span[0] != c;
    public static bool Is(this ReadOnlySpan<char> span, string s) =>
      span.SequenceEqual(s.AsSpan());
    public static bool IsNot(this ReadOnlySpan<char> span, string s) =>
      span.SequenceEqual(s.AsSpan());
    public static bool StartsWithInvariant(this ReadOnlySpan<char> str, string prefix) =>
      str.StartsWith(prefix.AsSpan(), StringComparison.OrdinalIgnoreCase);
    public static ReadOnlySpan<char> RemovePrefix(this ReadOnlySpan<char> str,
      string prefix, StringComparison compare = StringComparison.OrdinalIgnoreCase) =>
      str.StartsWith(prefix.AsSpan(), compare) ? str.Slice(prefix.Length) : str;
  }
}
