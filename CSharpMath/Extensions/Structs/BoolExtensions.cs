using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath {
  internal static class BoolExtensions {
    [System.Obsolete("Is any code using this?")]
    public static bool TryParse(string encodedValue) {
      bool r;
      string lowercaseEncodedValue = encodedValue.ToLowerInvariant().Trim();
      if (lowercaseEncodedValue.StartsWithInvariant("1") || lowercaseEncodedValue.StartsWithInvariant("y")
        || lowercaseEncodedValue.StartsWithInvariant("t")) {
        r = true;
      } else {
        r = false;
      }
      return r;
    }
  }
}
