using System;
using System.Globalization;

namespace CSharpMath {
  public static partial class Extensions {
    public static string ToStringInvariant<T>(this T value, string? format = null)
      where T : IFormattable => value.ToString(format, CultureInfo.InvariantCulture);
  }
}