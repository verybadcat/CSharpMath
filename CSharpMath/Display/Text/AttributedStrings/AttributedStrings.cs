using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Display.Text {
  [Obsolete("Is any code using this?", true)]
  public static class AttributedStrings {
    public static AttributedString<TFont, TGlyph> FromGlyphRuns<TFont, TGlyph>(params AttributedGlyphRun<TFont, TGlyph>[] runs)
      where TFont: IFont<TGlyph>
    => new AttributedString<TFont, TGlyph>(runs);
  }
}
