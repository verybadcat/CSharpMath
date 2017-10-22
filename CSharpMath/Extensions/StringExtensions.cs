using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath {
  public static class StringExtensions {
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
  }
}
