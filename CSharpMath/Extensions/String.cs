namespace CSharpMath {
  public static partial class Extensions {
    public static bool Contains(this string str, char c) {
      for (int i = 0; i < str.Length; i++)
        if (str[i] == c) return true;
      return false;
    }
    /// <summary>If the input is null, returns "null". Otherwise, returns the input.</summary>
    public static string NullToNull(this string? str, NullHandling handling) {
      if (str == null) {
        switch (handling) {
          case NullHandling.EmptyContent:
          case NullHandling.EmptyString:
            return string.Empty;
          case NullHandling.LiteralNull:
            return "null";
          default:
            throw new System.ComponentModel.InvalidEnumArgumentException
              (nameof(handling), (int)handling, typeof(NullHandling));
        }
      }
      return str;
    }
    public static string WrapInBraces(this string? str, NullHandling handling) =>
      str == null && handling == NullHandling.EmptyString
      ? string.Empty
      : $"{{{str.NullToNull(handling)}}}";
    public static string WrapInParens(this string? str, NullHandling handling) =>
      str == null && handling == NullHandling.EmptyString
      ? string.Empty
      : $"({str.NullToNull(handling)})";
    public static string WrapInSquareBrackets(this string? str, NullHandling handling) =>
      str == null && handling == NullHandling.EmptyString
      ? string.Empty
      : $"[{str.NullToNull(handling)}]";
  }
}
