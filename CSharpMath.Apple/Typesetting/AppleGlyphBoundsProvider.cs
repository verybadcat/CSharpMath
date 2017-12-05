using System;
using System.Drawing;
using CoreGraphics;
using CoreText;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using TGlyph = System.UInt16;
using TMathFont = CSharpMath.Apple.AppleMathFont;

namespace CSharpMath.Apple {
  public class AppleGlyphBoundsProvider: IGlyphBoundsProvider<TMathFont, TGlyph> {
    public AppleGlyphBoundsProvider() {
    }

    public float GetAdvancesForGlyphs(TMathFont font, ushort[] glyphs) {
      var ctFont = font.MyCTFont;
      var r = ctFont.GetAdvancesForGlyphs(CTFontOrientation.Default, glyphs);
      return (float)r;
    }

    public RectangleF GetBoundingRectForGlyphs(TMathFont font, ushort[] glyphs) {
      var cgRect = font.MyCTFont.GetBoundingRects(CTFontOrientation.Horizontal, glyphs);
      return (RectangleF)cgRect;
    }
  }
}
