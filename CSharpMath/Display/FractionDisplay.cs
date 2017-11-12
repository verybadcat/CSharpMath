using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display {
  public class FractionDisplay : DisplayBase {
    public MathListDisplay Numerator { get; private set; }
    public MathListDisplay Denominator { get; private set; }
  }
}
