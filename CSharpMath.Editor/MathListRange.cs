using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpMath.Editor {
  using Atoms;
  public readonly struct MathListRange {
    readonly MathListIndex _start;
    readonly int _length;
    public MathListRange(int start) => (_start, _length) = (MathListIndex.Level0Index(start), 1);
    public MathListRange(MathListIndex start) => (_start, _length) = (start, 1);
    public MathListRange(MathListIndex start, int length) => (_start, _length) = (start, length);
    public MathListRange(Range range) => (_start, _length) = (MathListIndex.Level0Index(range.Location), range.Length);

    public override string ToString() => $"({_start}, {_length})";
    public MathListRange? SubIndexRange => _start.SubIndexType != MathListSubIndexType.None ? new MathListRange(_start.SubIndex, _length) : new MathListRange?();
    public Range FinalRange => new Range(_start.FinalIndex, _length);

    public static MathListRange operator +(MathListRange left, MathListRange right) {
      if (left._start.AtSameLevel(right._start)) throw new InvalidOperationException($"Cannot union ranges at different levels: {left}, {right}");
      var leftRange = left.FinalRange;
      var rightRange = right.FinalRange;
      var unionRange = leftRange + rightRange;
      var start = unionRange.Location == leftRange.Location ? left._start : right._start;
      return new MathListRange(start, unionRange.Length);
    }

    public static MathListRange Combine(IEnumerable<MathListRange> ranges) => ranges.Aggregate((acc, curr) => acc + curr);
  }
}
