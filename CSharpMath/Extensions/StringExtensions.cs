using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CSharpMath {
  public static class StringExtensions {
    public static bool IsEmpty(this String str)
      => String.IsNullOrEmpty(str);

    public static bool IsNonEmpty(this string str)
      => !IsEmpty(str);
    /// <summary>If the input is null, returns "null". Otherwise, returns the input.</summary>
    public static string NullToNull(this string str, NullHandling handling) {
      if (str == null) {
        switch (handling) {
          case NullHandling.EmptyContent:
          case NullHandling.EmptyString:
            return "";
          case NullHandling.LiteralNull:
            return "null";
          case NullHandling.None:
          default:
            return null;
        }
      }
      return str;
    }
    public static string WrapInBraces(this string str, NullHandling handling) {
      if (str == null && handling == NullHandling.EmptyString) {
        return "";
      } else {
        return "{" + str.NullToNull(handling) + "}";
      }
    }

    public static string WrapInParens(this string str, NullHandling handling) {
      if (str == null && handling == NullHandling.EmptyString) {
        return "";
      } else {
        return "(" + str.NullToNull(handling) + ")";
      }
    }

    public static string WrapInSquareBrackets(this string str, NullHandling handling) {
      if (str == null && handling == NullHandling.EmptyString) {
        return "";
      } else {
        return "(" + str.NullToNull(handling) + ")";
      }
    }

    public static bool StartsWithInvariant(this string str, string prefix) {
      return str.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase);
    }

    public static string RemovePrefix(this string str, string prefix, StringComparison compare = StringComparison.InvariantCultureIgnoreCase) {
      if (str.StartsWith(prefix, compare)) {
        return str.Substring(prefix.Length);
      }
      return str;
    }

    public static void LogCharacters(this string str) {
      var chars = str.ToCharArray();
      Debug.WriteLine("Chars in " + str);
      foreach (var c in chars) {
        Debug.WriteLine((int)c);
      }
    }
  }
}
