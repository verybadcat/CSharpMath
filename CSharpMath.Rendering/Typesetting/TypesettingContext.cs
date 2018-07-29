using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Rendering {
  public static class TypesettingContext {
    public static FrontEnd.TypesettingContext<Fonts, Glyph> Instance { get; } =
      new FrontEnd.TypesettingContext<Fonts, Glyph>(
         //new FontMeasurer(),
         (fonts, size) => new Fonts(fonts, size),
         GlyphBoundsProvider.Instance,
         //new GlyphNameProvider(someTypefaceSizeIrrelevant),
         GlyphFinder.Instance,
         UnicodeFontChanger.Instance,
         //Resources.Json
         MathTable.Instance
       );
  }
}
