using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Rendering {
  public static class TypesettingContext {
    public static FrontEnd.TypesettingContext<MathFonts, Glyph> Instance { get; } =
      new FrontEnd.TypesettingContext<MathFonts, Glyph>(
         //new FontMeasurer(),
         (fonts, size) => new MathFonts(fonts, size),
         GlyphBoundsProvider.Instance,
         //new GlyphNameProvider(someTypefaceSizeIrrelevant),
         GlyphFinder.Instance,
         UnicodeFontChanger.Instance,
         //Resources.Json
         MathTable.Instance
       );
  }
}
