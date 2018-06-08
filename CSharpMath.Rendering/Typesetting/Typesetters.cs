using System;
using CSharpMath.FrontEnd;
using CSharpMath.Resources;
using Typography.OpenFont;

namespace CSharpMath.Rendering {
  public static class Typesetters {
    private static TypesettingContext<MathFont, Glyph> CreateTypesettingContext(Typeface typeface) =>
      new TypesettingContext<MathFont, Glyph>(
        //new SkiaFontMeasurer(),
        (font, size) => new MathFont(font, size),
        new GlyphBoundsProvider(),
        //new SkiaGlyphNameProvider(someTypefaceSizeIrrelevant),
        new GlyphFinder(typeface),
        new UnicodeFontChanger(),
        //SkiaResources.Json
        new MathTable(typeface)
      );

    private static TypesettingContext<MathFont, Glyph> CreateLatinMath() {
      var fontSize = 20;
      var skiaFont = FontManager.LatinMath(fontSize);
      return CreateTypesettingContext(skiaFont.Typeface);
    }

    private static TypesettingContext<MathFont, Glyph> _latinMath;
    private static readonly object _lock = new object();
    public static TypesettingContext<MathFont, Glyph> LatinMath {
      get {
        if (_latinMath == null) {
          lock(_lock) {
            if (_latinMath == null)
            {
              _latinMath = CreateLatinMath();
            }
          }
        }
        return _latinMath;
      }
    }
  }
}
