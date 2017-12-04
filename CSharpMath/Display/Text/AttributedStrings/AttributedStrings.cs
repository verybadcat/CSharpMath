using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Display.Text {
  public static class AttributedStrings {
    public static AttributedString<TMathFont, TGlyph> FromGlyphRuns<TMathFont, TGlyph>(params AttributedGlyphRun<TMathFont, TGlyph>[] runs)
      where TMathFont: MathFont<TGlyph>
    => new AttributedString<TMathFont, TGlyph>(runs);
  }
}
