using System;
using CSharpMath.FrontEnd;
using CSharpMath.Resources;
using Typography.OpenFont;
using TGlyph = System.UInt16;

namespace CSharpMath.SkiaSharp {
  public static class SkiaTypesetters {
    private static TypesettingContext<SkiaMathFont, TGlyph> CreateTypesettingContext(Typeface someTypefaceSizeIrrelevant) =>
      new TypesettingContext<SkiaMathFont, TGlyph>(
        new SkiaFontMeasurer(),
        (font, size) => new AppleMathFont(font, size),
        new SkiaGlyphBoundsProvider(),
        new SkiaGlyphNameProvider(someTypefaceSizeIrrelevant),
        new SkiaGlyphFinder(someTypefaceSizeIrrelevant),
        new UnicodeFontChanger(),
        IosResources.LatinMath
      );

    private static TypesettingContext<SkiaMathFont, TGlyph> CreateLatinMath() {
      var fontSize = 20;
      var skiaFont = SkiaFontManager.LatinMath(fontSize);
      return CreateTypesettingContext(skiaFont.Typeface);
    }

    private static TypesettingContext<SkiaMathFont, TGlyph> _latinMath;

    private static object _lock = new object();

    public static TypesettingContext<SkiaMathFont, TGlyph> LatinMath {
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
