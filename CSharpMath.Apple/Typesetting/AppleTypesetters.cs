using System;
using CSharpMath.FrontEnd;
using CSharpMath.Resources;
using Foundation;
using CoreText;
using TGlyph = System.UInt16;

namespace CSharpMath.Apple {
  public static class AppleTypesetters {
    public static TypesettingContext<AppleMathFont, TGlyph> CreateTypesettingContext(CTFont someCtFontSizeIrrelevant) {
      return new TypesettingContext<AppleMathFont, TGlyph>(
        new AppleFontMeasurer(),
        (font, size) => new AppleMathFont(font.Name, size),
        new AppleGlyphBoundsProvider(),
        new AppleGlyphNameProvider(someCtFontSizeIrrelevant),
        new UnicodeGlyphFinder(),
        ResourceLoader.LatinMath
      );
    }
  }
}
