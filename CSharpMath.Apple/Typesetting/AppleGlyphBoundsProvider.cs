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
    private readonly CtFontGlyphFinder _glyphFinder;
    public AppleGlyphBoundsProvider(CtFontGlyphFinder glyphFinder) {
      _glyphFinder = glyphFinder;
    }

    public float GetAdvancesForGlyphs(TFont font, TGlyph[] glyphs) {
      var ctFont = font.CtFont;
      var r = ctFont.GetAdvancesForGlyphs(CTFontOrientation.Default, glyphs);
      return (float)r;
    }

    public RectangleF[] GetBoundingRectsForGlyphs(TFont font, ushort[] glyphs, int nVariants)
    {
      CTFont ctFont = font.CtFont;
      CGRect[] rects = new CGRect[nVariants];
      ctFont.GetBoundingRects(CTFontOrientation.Horizontal, glyphs, rects, nVariants);
      RectangleF[] r = rects.Select(rect => (RectangleF)rect).ToArray();
      return r;
    }

    public double GetTypographicWidth(TFont font, AttributedGlyphRun<TFont, TGlyph> run) {
      var aString = run.ToNsAttributedString();
      var ctLine = new CTLine(aString);
      var typographicBounds = ctLine.GetTypographicBounds();
      ctLine.Dispose();
      return typographicBounds;
    }
  }
}
