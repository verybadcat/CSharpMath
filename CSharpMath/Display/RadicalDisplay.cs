using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display {
  public class RadicalDisplay : DisplayBase {
    public MathListDisplay Radicand { get; private set; }
    public MathListDisplay Degree { get; private set; }
  }
}
