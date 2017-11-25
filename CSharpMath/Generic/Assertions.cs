using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath {
  public static class Assertions {
    public static void Assert(bool condition, string message) {
      if (!condition) {
        throw new InvalidOperationException(message);
      }
    }
  }
}
