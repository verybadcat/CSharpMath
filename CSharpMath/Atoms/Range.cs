using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public struct Range {
    public const int NotFound = int.MinValue;
    public int Location { get; set; }
    /// <summary>
    /// The number of integers in the range. So End-Start=Length-1.
    /// </summary>
    public int Length { get; set; }

    public Range(int location, int length) {
      Location = location;
      Length = length;
    }

    public bool IsNotFound() => Location == NotFound || Length == NotFound;
    public int End {
      get {
        if (IsNotFound()) {
          return NotFound;
        }
        return Location + Length - 1;
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
  }
}
