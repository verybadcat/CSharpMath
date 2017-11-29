using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public struct Range {
    public const int UndefinedInt = int.MinValue;
    public static Range NotFoundRange = new Range(UndefinedInt, UndefinedInt);
    public int Location { get; set; }
    /// <summary>
    /// The number of integers in the range. So End-Start=Length-1.
    /// </summary>
    public int Length { get; set; }

    public Range(int location, int length) {
      Location = location;
      Length = length;
    }

    public bool IsNotFound() => Location == UndefinedInt || Length == UndefinedInt;
    public int End {
      get {
        if (IsNotFound()) {
          return UndefinedInt;
        }
        return Location + Length;
      }
    }
    public static bool operator == (Range range1, Range range2)
      => range1.Length == range2.Length && range1.Location == range2.Location;

    public static bool operator !=(Range range1, Range range2)
      => !(range1 == range2);

    public override bool Equals(object obj) => (obj is Range) && this == (Range)obj;

    public override int GetHashCode() {
      unchecked {
        return 13 * Length.GetHashCode() + Location.GetHashCode();
      }
    }

    public override string ToString() => "Range " + Location + " " + Length;
    internal float First() => throw new NotImplementedException();
  }
}
