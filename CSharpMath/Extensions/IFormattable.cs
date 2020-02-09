using System;
using System.Globalization;

namespace CSharpMath {
  public static class IFormattableExtensions {
    public static string ToStringInvariant<T>(this T value) where T : IFormattable =>
      value.ToString(null, CultureInfo.InvariantCulture);
  }
}