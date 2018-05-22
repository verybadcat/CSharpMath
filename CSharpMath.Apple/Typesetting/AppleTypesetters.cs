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
      var measurer = new AppleFontMeasurer();
      var nameProvider = new AppleGlyphNameProvider(someCtFontSizeIrrelevant);
      return new TypesettingContext<AppleMathFont, TGlyph>(
        measurer,
        (font, size) => new AppleMathFont(font, size),
        new AppleGlyphBoundsProvider(),
        nameProvider,
        new CtFontGlyphFinder(someCtFontSizeIrrelevant),
        new UnicodeFontChanger(),
        new JsonMathTable<AppleMathFont, TGlyph>(measurer, IosResources.LatinMath, nameProvider)
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
