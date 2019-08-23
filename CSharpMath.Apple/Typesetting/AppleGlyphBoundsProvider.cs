using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using CoreGraphics;
using CoreText;
using Foundation;
using UIKit;
using CSharpMath.Apple.Drawing;
using CSharpMath.Display;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using TGlyph = System.UInt16;
using TFont = CSharpMath.Apple.AppleMathFont;

namespace CSharpMath.Apple {
  public class AppleGlyphBoundsProvider: IGlyphBoundsProvider<TFont, TGlyph> {
    private AppleGlyphBoundsProvider() { }

    public static AppleGlyphBoundsProvider Instance { get; } = new AppleGlyphBoundsProvider();

    public (IEnumerable<float> Advances, float Total) GetAdvancesForGlyphs(TFont font, ForEach<TGlyph> glyphs, int nGlyphs) {
      var glyphArray = new TGlyph[nGlyphs];
      glyphs.CopyTo(glyphArray);
      var advanceSizes = new CGSize[nGlyphs];
      var combinedAdvance = font.CtFont.GetAdvancesForGlyphs(CTFontOrientation.Default, glyphArray, advanceSizes, nGlyphs);
      return (advanceSizes.Select(advance => (float)advance.Width), (float)combinedAdvance);
    }

    public IEnumerable<RectangleF> GetBoundingRectsForGlyphs(TFont font, ForEach<TGlyph> glyphs, int nVariants) {
      var glyphArray = new TGlyph[nVariants];
      glyphs.CopyTo(glyphArray);
      var rects = new CGRect[nVariants];
      font.CtFont.GetBoundingRects(CTFontOrientation.Horizontal, glyphArray, rects, nVariants);
      return rects.Select(rect => (RectangleF)rect);
    }

    public float GetTypographicWidth(TFont font, AttributedGlyphRun<TFont, TGlyph> run) {
      using (var aString = run.ToNsAttributedString())
      using (var ctLine = new CTLine(aString))
        return (float)ctLine.GetTypographicBounds();
    }
  }
}
