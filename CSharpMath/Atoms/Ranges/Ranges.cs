using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public static class Ranges {
    public static Range NotFound => new Range {
      Location = Range.UndefinedInt,
      Length = Range.UndefinedInt
    };
    public static Range Zero => new Range {
      Location = 0,
      Length = 0
    };
  }
}
