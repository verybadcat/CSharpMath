using System.Drawing;
using System.Linq;
using Typography.OpenFont;
using static Typography.Contours.MyMath;
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
      var rects = new RectangleF[glyphs.Length];
      for (int i = 0; i < glyphs.Length; i++) {
        var bounds = glyphs[i].Bounds;
        var scale = font.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize);
        rects[i] = new RectangleF(bounds.XMin * scale, bounds.YMin * scale,
          (bounds.XMax - bounds.XMin) * scale, (bounds.YMax - bounds.YMin) * scale);
      }
      return rects;
    }

    public float GetTypographicWidth(TFont font, AttributedGlyphRun<TFont, Glyph> run) {
      var stringBox = font.GlyphLayout.LayoutAndMeasureString(run.Text.ToCharArray(), 0, run.Length, font.PointSize);
      return stringBox.width + run.KernedGlyphs.Sum(g => g.KernAfterGlyph);
    }
  }
}
