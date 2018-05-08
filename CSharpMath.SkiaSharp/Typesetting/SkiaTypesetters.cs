using System;
using CSharpMath.FrontEnd;
using CSharpMath.Resources;
using Typography.OpenFont;

namespace CSharpMath.SkiaSharp {
  public static class SkiaTypesetters {
    private static TypesettingContext<SkiaMathFont, Glyph> CreateTypesettingContext(Typeface someTypefaceSizeIrrelevant) =>
      new TypesettingContext<SkiaMathFont, Glyph>(
        new SkiaFontMeasurer(),
        (font, size) => new SkiaMathFont(font, size),
        new SkiaGlyphBoundsProvider(),
        new SkiaGlyphNameProvider(someTypefaceSizeIrrelevant),
        new SkiaGlyphFinder(someTypefaceSizeIrrelevant),
        new UnicodeFontChanger(),
        SkiaResources.Json
      );

    private static TypesettingContext<SkiaMathFont, Glyph> CreateLatinMath() {
      var fontSize = 20;
      var skiaFont = SkiaFontManager.LatinMath(fontSize);
      return CreateTypesettingContext(skiaFont.Typeface);
    }

    private static TypesettingContext<SkiaMathFont, Glyph> _latinMath;
    private static readonly object _lock = new object();
    public static TypesettingContext<SkiaMathFont, Glyph> LatinMath {
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
