using System;
using CSharpMath.FrontEnd;
using CSharpMath.Resources;
using Foundation;
using CoreText;
using TGlyph = System.UInt16;
using CSharpMath.Ios.Resources;

namespace CSharpMath.Apple {
  public static class AppleTypesetters {
    private static TypesettingContext<AppleMathFont, TGlyph> CreateTypesettingContext(CTFont someCtFontSizeIrrelevant) {
      var glyphFinder = new CtFontGlyphFinder(someCtFontSizeIrrelevant);
      return new TypesettingContext<AppleMathFont, TGlyph>(
        new AppleFontMeasurer(),
        (font, size) => new AppleMathFont(font, size),
        new AppleGlyphBoundsProvider(glyphFinder),
        new AppleGlyphNameProvider(someCtFontSizeIrrelevant),
        glyphFinder,
        new UnicodeFontChanger(),
        IosResources.LatinMath
      );
    }

    private static TypesettingContext<AppleMathFont, TGlyph> CreateLatinMath() {
      var fontSize = 20;
      var appleFont = AppleFontManager.LatinMath(fontSize);
      return CreateTypesettingContext(appleFont.CtFont);
    }

    private static TypesettingContext<AppleMathFont, TGlyph> _latinMath;

    private static object _lock = new object();

    public static TypesettingContext<AppleMathFont, TGlyph> LatinMath {
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
