using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Models {
  public static class Ranges {
    public static Range NotFound => new Range {
      Location = Range.NotFound,
      Length = Range.NotFound
    };
  }
}
