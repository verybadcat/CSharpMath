using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpMath.Atom {
  // e.g. new Range(location: 1, length: 3)
  // 0     1     2     3     4 (End exclusive)
  //       ^----------------^ Length = 3
  //     Start              End
  /// <summary>
  /// Corresponds to a range of <see cref="MathAtom"/>s before finalization.
  /// This value is tracked in finalized <see cref="MathAtom"/>s and <see cref="Display.IDisplay{TFont, TGlyph}"/>s,
  /// for utilization in CSharpMath.Editor to construct MathListIndexes from <see cref="Display.IDisplay{TFont, TGlyph}"/>s.
  /// </summary>
  public readonly struct Range : IEquatable<Range> {
    public const int UndefinedInt = int.MinValue;
    /// <summary>Value of IndexRange for unfinalized atoms</summary>
    public static readonly Range Zero = new Range(0, 0);
    public static readonly Range NotFound = new Range(UndefinedInt, UndefinedInt);
    public int Location { get; }
    /// <summary>The number of integers in the range. So End-Start=Length-1.</summary>
    public int Length { get; }
    public Range(int location, int length) {
      Location = location;
      Length = length;
    }
    public bool IsNotFound => Location == UndefinedInt || Length == UndefinedInt;
    /// <summary>The end is exclusive.</summary>
    public int End => IsNotFound ? UndefinedInt : Location + Length;
    public static bool operator ==(Range range1, Range range2) =>
      range1.Length == range2.Length && range1.Location == range2.Location;
    public static bool operator !=(Range range1, Range range2) => !(range1 == range2);
    public override bool Equals(object obj) => obj is Range r && this == r;
    public bool Equals(Range r) => this == r;
    /// <summary>If either Range is NotFound, returns the other.
    /// Otherwise, combines the ranges.</summary>
    public static Range operator +(Range range1, Range range2) {
      if (range1.IsNotFound) return range2;
      if (range2.IsNotFound) return range1;
      var start = Math.Min(range1.Location, range2.Location);
      var end = Math.Max(range1.End, range2.End);
      return new Range(start, end - start);
    }
    public static Range Add(Range range1, Range range2) => range1 + range2;
    public override int GetHashCode() => unchecked(13 * Length.GetHashCode() + Location.GetHashCode());
    public override string ToString() => $@"{{{Location}, {Length}}}";
    public bool Contains(int i) => i >= Location && i < End;
    public Range Slice(int start, int length) => new Range(Location + start, length);
    public static Range Combine(IEnumerable<Range> ranges) {
      var trimRanges = ranges.Where(r => !r.IsNotFound).ToList();
      if (trimRanges.Count == 0) return NotFound;
      int start = trimRanges.Min(r => r.Location);
      int end = trimRanges.Max(r => r.End);
      return new Range(start, end - start);
    }
  }
}
