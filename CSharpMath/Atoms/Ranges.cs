using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public static class Ranges {
    public static Range NotFound => new Range {
      Location = Range.NotFound,
      Length = Range.NotFound
    };
  }
}
