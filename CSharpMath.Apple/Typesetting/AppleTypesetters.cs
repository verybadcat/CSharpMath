using System;
using CSharpMath.FrontEnd;
using CSharpMath.Resources;
using Foundation;
using TGlyph = System.UInt16;
using Newtonsoft.Json.Linq;
using CoreText;

namespace CSharpMath.Apple {
  public static class AppleTypesetters {
    public static TypesettingContext<AppleMathFont, TGlyph> CreateTypesettingContext(CTFont someCtFontSizeIrrelevant) {
      return new TypesettingContext<AppleMathFont, TGlyph>(
        new AppleFontMeasurer(),
        (font, size) => new AppleMathFont(font.Name, size),
        new AppleGlyphBoundsProvider(),
        new AppleGlyphNameProvider(someCtFontSizeIrrelevant),
        new AppleGlyphFinder(),
        ResourceLoader.LatinMath
      );
    }
  }
}
