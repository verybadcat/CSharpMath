using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath.Atoms {
  public static class RangeExtensions {
    /// <summary>
    /// An
    /// </summary>
    /// <param name="ranges"></param>
    /// <returns></returns>
    public static Range Combine(IEnumerable<Range> ranges) {
      var trimRanges = ranges.Where(r => !r.IsNotFound()).ToList();
      if (trimRanges.Count == 0) {
        return Ranges.NotFound;
      }
      int start = trimRanges.Min(r => r.Location);
      int end = trimRanges.Max(r => r.End);
      return new Range(start, end - start);
    }
    public static Range Combine(Range range1, Range range2)
      => Combine(new List<Range> { range1, range2 });
  }
}
