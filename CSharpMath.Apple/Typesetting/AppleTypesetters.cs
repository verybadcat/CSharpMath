using CSharpMath.FrontEnd;
using TGlyph = System.UInt16;
using CSharpMath.Ios.Resources;

namespace CSharpMath.Apple {
  public static class AppleTypesetters {
    private static TypesettingContext<AppleMathFont, TGlyph> CreateTypesettingContext(CoreGraphics.CGFont someCgFontSizeIrrelevant) {
      var boundsProvider = AppleGlyphBoundsProvider.Instance;
      return new TypesettingContext<AppleMathFont, TGlyph>(
        (font, size) => new AppleMathFont(font, size),
        boundsProvider,
        CtFontGlyphFinder.Instance,
        UnicodeFontChanger.Instance,
        new JsonMathTable<AppleMathFont, TGlyph>(AppleFontMeasurer.Instance, IosResources.LatinMath, new AppleGlyphNameProvider(someCgFontSizeIrrelevant), boundsProvider)
      );
    }

    private static TypesettingContext<AppleMathFont, TGlyph> CreateLatinMath() {
      var fontSize = 20;
      var appleFont = AppleFontManager.LatinMath(fontSize);
      return CreateTypesettingContext(appleFont.CgFont);
    }

    private static TypesettingContext<AppleMathFont, TGlyph> _latinMath;

    private static readonly object _lock = new object();

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
