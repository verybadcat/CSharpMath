using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpMath.Atoms {
  public readonly struct Range {
    public const int UndefinedInt = int.MinValue;
    public static readonly Range NotFound = new Range(UndefinedInt, UndefinedInt);
    public static readonly Range Zero = new Range(0, 0);
    public int Location { get; }
    /// <summary>
    /// The number of integers in the range. So End-Start=Length-1.
    /// </summary>
    public int Length { get; }

    public Range(int location, int length) {
      Location = location;
      Length = length;
    }

    public bool IsNotFound => Location == UndefinedInt || Length == UndefinedInt;
    public int End => IsNotFound ? UndefinedInt : Location + Length;

    public static bool operator ==(Range range1, Range range2) => range1.Length == range2.Length && range1.Location == range2.Location;
    public static bool operator !=(Range range1, Range range2) => !(range1 == range2);
    /// <summary>If either Range is NotFound, returns the other.
    /// Otherwise, combines the ranges.</summary>
    public static Range operator +(Range range1, Range range2) {
      if (range1.IsNotFound) return range2;
      if (range2.IsNotFound) return range1;
      var start = Math.Min(range1.Location, range2.Location);
      var end = Math.Max(range1.End, range2.End);
      return new Range(start, end - start);
    }

    public override bool Equals(object obj) => obj is Range r && this == r;
    public override int GetHashCode() => unchecked(13 * Length.GetHashCode() + Location.GetHashCode());
    public override string ToString() => $@"{{{Location}, {Length}}}";
    public bool Contains(int i) => i >= Location && i <= End;

    public static Range Combine(IEnumerable<Range> ranges) {
      var trimRanges = ranges.Where(r => !r.IsNotFound).ToList();
      if (trimRanges.Count == 0) return NotFound;
      int start = trimRanges.Min(r => r.Location);
      int end = trimRanges.Max(r => r.End);
      return new Range(start, end - start);
    }
  }
}
