using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpMath.Editor {
  using Atom;
  public readonly record struct MathListRange(MathListIndex Start, int Length) {
    public MathListRange? SubIndexRange => Start.SubIndexInfo is var (_, subIndex) ? new(subIndex, Length) : null;
    public Range FinalRange => new(Start.FinalIndex, Length);
    public override string ToString() => $"({Start}, {Length})";
    public static MathListRange operator +(MathListRange left, MathListRange right) {
      if (!left.Start.AtSameLevel(right.Start))
        throw new InvalidOperationException($"Cannot union ranges at different levels: {left}, {right}");
      var leftRange = left.FinalRange;
      var rightRange = right.FinalRange;
      var unionRange = leftRange + rightRange;
      return new MathListRange(unionRange.Location == leftRange.Location ? left.Start : right.Start, unionRange.Length);
    }
    public static MathListRange Combine(IEnumerable<MathListRange> ranges) => ranges.Aggregate((acc, curr) => acc + curr);
  }
}