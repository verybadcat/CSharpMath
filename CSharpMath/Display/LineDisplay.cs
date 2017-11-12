using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display {
  public class LineDisplay : DisplayBase {
    /// <summary>A display representing the inner list that is underlined.
    /// Its position is relative to the parent. Is not treated as a
    /// sub-display.</summary> 
    public MathListDisplay Inner { get; private set; }
  }
}
