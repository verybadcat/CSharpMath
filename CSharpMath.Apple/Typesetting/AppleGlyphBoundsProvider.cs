using System;
using System.Diagnostics;
using System.Drawing;
using CoreGraphics;
using CoreText;
using Foundation;
using UIKit;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using TGlyph = System.UInt16;
using TFont = CSharpMath.Apple.AppleMathFont;
using System.Linq;
using CSharpMath.Display;
using CSharpMath.Apple.Drawing;

namespace CSharpMath.Apple {
  public class AppleGlyphBoundsProvider: IGlyphBoundsProvider<TFont, TGlyph> {

    public (float[] Advances, float Total) GetAdvancesForGlyphs(TFont font, TGlyph[] glyphs) {
      var ctFont = font.CtFont;
      var nGlyphs = glyphs.Length;
      var advanceSizes = new CGSize[nGlyphs];
      var combinedAdvance = ctFont.GetAdvancesForGlyphs(CTFontOrientation.Default, glyphs, advanceSizes, nGlyphs);
      var advances = new float[nGlyphs];
      for (int i = 0; i < nGlyphs; i++) advances[i] = (float)advanceSizes[i].Width;
      return (advances, (float)combinedAdvance);
    }

    public RectangleF[] GetBoundingRectsForGlyphs(TFont font, TGlyph[] glyphs)
    {
      CTFont ctFont = font.CtFont;
      int nVariants = glyphs.Length;
      CGRect[] rects = new CGRect[nVariants];
      ctFont.GetBoundingRects(CTFontOrientation.Horizontal, glyphs, rects, nVariants);
      RectangleF[] r = rects.Select(rect => (RectangleF)rect).ToArray();
      return r;
    }

    public float GetTypographicWidth(TFont font, AttributedGlyphRun<TFont, TGlyph> run) {
      var aString = run.ToNsAttributedString();
      using (var ctLine = new CTLine(aString))
        return (float)ctLine.GetTypographicBounds();
    }
  }
}
