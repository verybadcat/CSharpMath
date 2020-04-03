using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CoreGraphics;
using CoreText;
using CSharpMath.Display;
using CSharpMath.Display.FrontEnd;
using TGlyph = System.UInt16;
using TFont = CSharpMath.Apple.AppleMathFont;

namespace CSharpMath.Apple {
  public class AppleGlyphBoundsProvider: IGlyphBoundsProvider<TFont, TGlyph> {
    private AppleGlyphBoundsProvider() { }
    public static AppleGlyphBoundsProvider Instance { get; } = new AppleGlyphBoundsProvider();
    public (IEnumerable<float> Advances, float Total) GetAdvancesForGlyphs
      (TFont font, IEnumerable<TGlyph> glyphs, int nGlyphs) {
      using var glyphArray = new Structures.RentedArray<TGlyph>(glyphs, nGlyphs);
      var advanceSizes = new CGSize[nGlyphs];
      var combinedAdvance = font.CtFont.GetAdvancesForGlyphs
        (CTFontOrientation.Default, glyphArray.Result.Array, advanceSizes, nGlyphs);
      return (advanceSizes.Select(advance => (float)advance.Width), (float)combinedAdvance);
    }
    public IEnumerable<RectangleF> GetBoundingRectsForGlyphs(TFont font, IEnumerable<TGlyph> glyphs, int nVariants) {
      using var glyphArray = new Structures.RentedArray<TGlyph>(glyphs, nVariants);
      var rects = new CGRect[nVariants];
      font.CtFont.GetBoundingRects(CTFontOrientation.Horizontal, glyphArray.Result.Array, rects, nVariants);
      return rects.Select(rect => (RectangleF)rect);
    }
    public float GetTypographicWidth(TFont font, AttributedGlyphRun<TFont, TGlyph> run) {
      using var aString = run.ToNsAttributedString();
      using var ctLine = new CTLine(aString);
      return (float)ctLine.GetTypographicBounds();
    }
  }
}
