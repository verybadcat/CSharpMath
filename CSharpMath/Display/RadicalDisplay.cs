using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display {
  public class RadicalDisplay : DisplayBase {
    // A display representing the numerator of the fraction. Its position is relative
    // to the parent and it is not treated as a sub-display.
    public MathListDisplay Radicand { get; private set; }
    // A display representing the numerator of the fraction. Its position is relative
    // to the parent and it is not treated as a sub-display.
    public MathListDisplay Degree { get; private set; }
  }
}
