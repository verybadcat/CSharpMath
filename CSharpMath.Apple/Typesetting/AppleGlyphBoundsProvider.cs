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
using TMathFont = CSharpMath.Apple.AppleMathFont;

namespace CSharpMath.Apple {
  public class AppleGlyphBoundsProvider: IGlyphBoundsProvider<TMathFont, TGlyph> {
    private readonly CtFontGlyphFinder _glyphFinder;
    public AppleGlyphBoundsProvider(CtFontGlyphFinder glyphFinder) {
      _glyphFinder = glyphFinder;
    }

    public float GetAdvancesForGlyphs(TMathFont font, ushort[] glyphs) {
      var ctFont = font.CtFont;
      var r = ctFont.GetAdvancesForGlyphs(CTFontOrientation.Default, glyphs);
      return (float)r;
    }

    public RectangleF GetBoundingRectForGlyphs(TMathFont font, ushort[] glyphs) {
      int nGlyphs = glyphs.Length;
      var rects = new CGRect[nGlyphs];
      var sizes = new CGSize[nGlyphs];
      var fontSize = font.CtFont.Size;
      var cgRect = font.CtFont.GetBoundingRects(CTFontOrientation.Default, glyphs, rects, nGlyphs);
      var advances = font.CtFont.GetAdvancesForGlyphs(CTFontOrientation.Default, glyphs, sizes, nGlyphs);
      CGRect outputRect = CGRect.Empty;
      nfloat x = 0;
      nfloat y = 0;
      for (int i = 0; i < nGlyphs; i++) {
        var glyphRect = new CGRect(
          rects[i].X + x,
          rects[i].Y + y,
          rects[i].Width,
          rects[i].Height
        );
        x += sizes[i].Width;
        y += sizes[i].Height;
        if (outputRect == CGRect.Empty) {
          outputRect = glyphRect;
        }
        else {
          outputRect = CGRect.Union(outputRect, glyphRect);
        }
      }
      var text = _glyphFinder.FindStringDebugPurposesOnly(glyphs);
      return (RectangleF)outputRect;
    }
  }
}
