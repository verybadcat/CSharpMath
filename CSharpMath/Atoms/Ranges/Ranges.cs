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
    /// <summary>If either Range is NotFound, returns the other.
    /// Otherwise, combines the ranges.</summary>
    public static Range Union(Range range1, Range range2) {
      if (range1.IsNotFound()) {
        return range2;
      }
      if (range2.IsNotFound()) {
        return range1;
      }
      var start = Math.Min(range1.Location, range2.Location);
      var end = Math.Max(range1.End, range2.End);
      return new Range(start, end - start);
    }
  }
}
