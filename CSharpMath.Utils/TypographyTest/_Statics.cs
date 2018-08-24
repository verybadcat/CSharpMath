using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.DevUtils.TypographyTest {
  static class _Statics {
    public static Typography.TextLayout.GlyphLayout GlyphLayout => new Typography.TextLayout.GlyphLayout() {
      Typeface = new Typography.OpenFont.OpenFontReader().Read(new System.IO.FileStream(Paths.LatinModernMathSource, System.IO.FileMode.Open))    };
  }
}
