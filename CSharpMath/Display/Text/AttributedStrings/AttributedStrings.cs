using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Display.Text {
  public static class AttributedStrings {
    public static AttributedString<TFont, TGlyph> FromGlyphRuns<TFont, TGlyph>(params AttributedGlyphRun<TFont, TGlyph>[] runs)
      where TFont: MathFont<TGlyph>
    => new AttributedString<TFont, TGlyph>(runs);
  }
}
