using System.Globalization;
using numericType = System.Double;

namespace CSharpMath.Extensions.NumericTypes
{
  internal static class DoubleExtensions {
    public static numericType TryParse(string s, numericType failValue = numericType.NaN) {
      numericType dub;
      numericType r = failValue;
      if (s?.Trim() == "") {
        r = 0;
      } else {
        bool success = numericType.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out dub);
        if (success) {
          r = dub;
        }
      }
      return r;
    }
  }
}
