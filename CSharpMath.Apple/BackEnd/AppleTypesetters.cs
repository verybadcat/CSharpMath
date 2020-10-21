using CSharpMath.Display.FrontEnd;
using TGlyph = System.UInt16;

namespace CSharpMath.Apple {
  public static class AppleTypesetters {
    public static TypesettingContext<AppleMathFont, TGlyph> LatinMath { get; } =
      new TypesettingContext<AppleMathFont, TGlyph>(
        (font, size) => new AppleMathFont(font, size),
        AppleGlyphBoundsProvider.Instance,
        CtFontGlyphFinder.Instance,
        new JsonMathTable<AppleMathFont, TGlyph>(
          AppleFontMeasurer.Instance,
          Resources.ManifestResources.LatinMath,
          new AppleGlyphNameProvider(AppleMathFont.LatinMath(20).CgFont),
          AppleGlyphBoundsProvider.Instance)
      );
  }
}