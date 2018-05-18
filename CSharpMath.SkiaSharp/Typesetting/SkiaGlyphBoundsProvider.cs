using System.Drawing;
using System.Linq;
using Typography.OpenFont;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using TFont = CSharpMath.SkiaSharp.SkiaMathFont;

namespace CSharpMath.SkiaSharp {
  public class SkiaGlyphBoundsProvider: IGlyphBoundsProvider<TFont, Glyph> {

    public float[] GetAdvancesForGlyphs(TFont font, Glyph[] glyphs) {
      var typeface = font.Typeface;
      var nGlyphs = glyphs.Length;
      var advanceSizes = new float[nGlyphs + 1];
      for (int i = 0; i < nGlyphs; i++) {
        advanceSizes[i] = glyphs[i].OriginalAdvanceWidth;
      }
      var combinedAdvance = advanceSizes.Sum();
      advanceSizes[nGlyphs] = combinedAdvance;
      return advanceSizes;
    }

    public RectangleF[] GetBoundingRectsForGlyphs(TFont font, Glyph[] glyphs, int nVariants)
    {
      var typeface = font.Typeface;
      var rects = new RectangleF[glyphs.Length];
      for (int i = 0; i < glyphs.Length; i++) {
        var bounds = glyphs[i].Bounds;
        rects[i] = RectangleF.FromLTRB(bounds.XMin, bounds.YMin, bounds.XMax, bounds.YMax);
      }
      return rects;
    }

    public float GetTypographicWidth(TFont font, AttributedGlyphRun<TFont, Glyph> run) {
      var stringBox = font.GlyphLayout.LayoutAndMeasureString(run.Text.ToCharArray(), 0, run.Length, font.PointSize, out var _);
      return stringBox.width + run.KernedGlyphs.Sum(g => g.KernAfterGlyph);
    }
  }
}
