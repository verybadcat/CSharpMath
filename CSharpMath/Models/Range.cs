using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Models {
  public struct Range {
    public const int NotFound = int.MinValue;
    public int Location { get; set; }
    /// <summary>
    /// The number of integers in the range. So End-Start=Length-1.
    /// </summary>
    public int Length { get; set; }

    public bool IsNotFound() => Location == NotFound || Length == NotFound;
    public int End {
      get {
        if (IsNotFound()) {
          return NotFound;
        }
        return Location + Length - 1;
      }
    }
  }
}
