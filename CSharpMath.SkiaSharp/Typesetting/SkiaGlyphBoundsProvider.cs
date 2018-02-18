using System;
using SkiaSharp;
using System.Drawing;
using Typography.OpenFont;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using TFont = CSharpMath.SkiaSharp.SkiaMathFont;
using TGlyph = System.Int32;
using System.Linq;
using CSharpMath.Display;
using CSharpMath.SkiaSharp.Drawing;

namespace CSharpMath.SkiaSharp {
  public class SkiaGlyphBoundsProvider: IGlyphBoundsProvider<TFont, TGlyph> {

    public float[] GetAdvancesForGlyphs(TFont font, TGlyph[] glyphs) {
      var typeface = font.Typeface;
      var nGlyphs = typeface.Glyphs.Length;
      var advanceSizes = new float[nGlyphs + 1];
      for (int i = 0; i < nGlyphs; i++) {
        advanceSizes[i] = typeface.GetHAdvanceWidthFromGlyphIndex(glyphs[i]);
      }
      var combinedAdvance = advanceSizes.Sum();
      advanceSizes[nGlyphs] = combinedAdvance;
      return advanceSizes;
    }

    public RectangleF[] GetBoundingRectsForGlyphs(TFont font, TGlyph[] glyphs, int nVariants)
    {
      var typeface = font.Typeface;
      var rects = new RectangleF[glyphs.Length];
      for (int i = 0; i < glyphs.Length; i++) {
        var bounds = font.Typeface.GetGlyphByIndex(glyphs[i]).Bounds;
        rects[i] = RectangleF.FromLTRB(bounds.XMin, bounds.YMin, bounds.XMax, bounds.YMax);
      }
      return rects;
    }

    public double GetTypographicWidth(TFont font, AttributedGlyphRun<TFont, TGlyph> run) {
      SKTypeface.FromFile(null).
      var aString = run.ToNsAttributedString();
      var ctLine = new CTLine(aString);
      var typographicBounds = ctLine.GetTypographicBounds();
      ctLine.Dispose();
      return typographicBounds;
    }
  }
}
