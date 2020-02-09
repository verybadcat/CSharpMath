using CSharpMath.FrontEnd;
using TGlyph = System.UInt16;

namespace CSharpMath.Apple {
  public static class AppleTypesetters {
    private static TypesettingContext<AppleMathFont, TGlyph> CreateTypesettingContext
      (CoreGraphics.CGFont someCgFontSizeIrrelevant) {
      var boundsProvider = AppleGlyphBoundsProvider.Instance;
      return new TypesettingContext<AppleMathFont, TGlyph>(
        (font, size) => new AppleMathFont(font, size),
        boundsProvider,
        CtFontGlyphFinder.Instance,
        Displays.UnicodeFontChanger.Instance,
        new JsonMathTable<AppleMathFont, TGlyph>(
          AppleFontMeasurer.Instance,
          Resources.ManifestResources.LatinMath,
          new AppleGlyphNameProvider(someCgFontSizeIrrelevant),
          boundsProvider)
      );
    }
    public static TypesettingContext<AppleMathFont, TGlyph> LatinMath { get; } =
      CreateTypesettingContext(AppleFontManager.LatinMath(20).CgFont);
  }
}