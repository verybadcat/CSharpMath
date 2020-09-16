using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Forms.Tests {
  public static class Extensions {
    public static T NotNull<T>(this T? obj) where T : class {
      if (obj == null)
        throw new Xunit.Sdk.NotNullException();
#nullable disable
      return obj;
#nullable restore
    }
  }
}
