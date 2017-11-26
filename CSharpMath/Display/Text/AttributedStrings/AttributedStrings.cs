using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Display.Text {
  public static class AttributedStrings {
    public static AttributedString FromGlyphRuns(params AttributedGlyphRun[] runs)
      => new AttributedString(runs);
  }
}
