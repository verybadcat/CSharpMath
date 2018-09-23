using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CSharpMath {
  using Enumerations;
  public static class StringExtensions {
    public static bool IsEmpty(this string str) => string.IsNullOrEmpty(str);
    public static bool IsNonEmpty(this string str) => !IsEmpty(str);

    /// <summary>If the input is null, returns "null". Otherwise, returns the input.</summary>
    public static string NullToNull(this string str, NullHandling handling) {
      if (str == null) {
        switch (handling) {
          case NullHandling.EmptyContent:
          case NullHandling.EmptyString:
            return string.Empty;
          case NullHandling.LiteralNull:
            return "null";
          case NullHandling.None:
          default:
            return null;
        }
      }
      return str;
    }
    public static string WrapInBraces(this string str, NullHandling handling) =>
      str == null && handling == NullHandling.EmptyString ? string.Empty : $"{{{str.NullToNull(handling)}}}";

    public static string WrapInParens(this string str, NullHandling handling) =>
      str == null && handling == NullHandling.EmptyString ? string.Empty : $"({str.NullToNull(handling)})";

    public static string WrapInSquareBrackets(this string str, NullHandling handling) =>
      str == null && handling == NullHandling.EmptyString ? string.Empty : $"[{str.NullToNull(handling)}]";

    public static bool StartsWithInvariant(this ReadOnlySpan<char> str, string prefix) =>
      str.StartsWith(prefix.AsSpan(), StringComparison.OrdinalIgnoreCase);

    public static ReadOnlySpan<char> RemovePrefix(this ReadOnlySpan<char> str, string prefix, StringComparison compare = StringComparison.OrdinalIgnoreCase) =>
      str.StartsWith(prefix.AsSpan(), compare) ? str.Slice(prefix.Length) : str;

    public static void LogCharacters(this string str) {
      var chars = str.ToCharArray();
      Debug.WriteLine("Chars in " + str);
      foreach (var c in chars)
        Debug.WriteLine((int)c);
    }
  }
}
