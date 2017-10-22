using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public static class Ranges {
    public static Range NotFound => new Range {
      Location = Range.NotFound,
      Length = Range.NotFound
    };
    public static Range Zero => new Range {
      Location = 0,
      Length = 0
    };
  }
}
