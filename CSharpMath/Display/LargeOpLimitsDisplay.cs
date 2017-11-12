using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display {
  public class LargeOpLimitsDisplay : DisplayBase {
    public MathListDisplay UpperLimit { get; private set; }
    public MathListDisplay LowerLimit { get; private set; }
  }
}
