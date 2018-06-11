using System.Drawing;
using System.Linq;
using Typography.OpenFont;
using static Typography.Contours.MyMath;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using TFonts = CSharpMath.Rendering.MathFonts;

namespace CSharpMath.Rendering {
  public class GlyphBoundsProvider: IGlyphBoundsProvider<TFonts, Glyph> {

    public (float[] Advances, float Total) GetAdvancesForGlyphs(TFonts fonts, Glyph[] glyphs) {
      var nGlyphs = glyphs.Length;
      var advanceSizes = new float[nGlyphs];
      var total = 0f;
      var i = 0;
      foreach (var (typeface, glyph) in glyphs) {
        var scale = typeface.CalculateScaleToPixelFromPointSize(fonts.PointSize);
        total += advanceSizes[i] = typeface.GetHAdvanceWidthFromGlyphIndex(glyph.GlyphIndex) * scale;
        i++;
      }
      return (advanceSizes, total);
    }

    public RectangleF[] GetBoundingRectsForGlyphs(TFonts font, Glyph[] glyphs)
    {
      var rects = new RectangleF[glyphs.Length];
      var i = 0;
      foreach (var (typeface, glyph) in glyphs) {
        var scale = typeface.CalculateScaleToPixelFromPointSize(font.PointSize);
        var bounds = glyph.Bounds;
        var obounds = glyph.GetOriginalBounds();
        rects[i] = RectangleF.FromLTRB(obounds.XMin * scale, bounds.YMin * scale, obounds.XMax * scale, bounds.YMax * scale);
        i++;
      }
      return rects;
    }

    public float GetTypographicWidth(TFonts font, AttributedGlyphRun<TFonts, Glyph> run) {
      var stringBox = font.GlyphLayout.LayoutAndMeasureString(run.Text.ToCharArray(), 0, run.Text.Length, font.PointSize);
      return stringBox.width + run.KernedGlyphs.Sum(g => g.KernAfterGlyph);
    }
  }
}
