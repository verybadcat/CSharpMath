using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath.Atoms {
  public static class RangeExtensions {
    public static Range Combine(IEnumerable<Range> ranges) {
      if (ranges.IsEmpty()) {
        return Ranges.NotFound;
      }
      if (ranges.Any(r => r.IsNotFound())) {
        return Ranges.NotFound;
      }
      int start = ranges.Min(r => r.Location);
      int end = ranges.Max(r => r.End);
      return new Range(start, end - start);
    }
    public static Range Combine(Range range1, Range range2)
      => Combine(new List<Range> { range1, range2 });
  }
}
