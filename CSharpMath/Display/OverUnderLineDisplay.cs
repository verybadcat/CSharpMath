using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display {
  /// <summary>Corresponds to MTLineDisplay in iosMath.</summary> 
  public class OverOrUnderlineDisplay : DisplayBase {
    /// <summary>A display representing the inner list that is underlined.
    /// Its position is relative to the parent. Is not treated as a
    public MathListDisplay Inner { get; private set; }
    /// sub-display.</summary> 
  }
}
