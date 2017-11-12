using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display {
  public class AccentDisplay : DisplayBase {
    public MathListDisplay Accentee { get; private set; }
    public GlyphDisplay Accent { get; private set; }
  }
}
