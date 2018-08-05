using System.Drawing;
using System.Linq;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using TFonts = CSharpMath.Rendering.Fonts;

namespace CSharpMath.Rendering {
  public class GlyphBoundsProvider : IGlyphBoundsProvider<TFonts, Glyph> {
    private GlyphBoundsProvider() { }

    public static GlyphBoundsProvider Instance { get; } = new GlyphBoundsProvider();

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

    public RectangleF[] GetBoundingRectsForGlyphs(TFonts fonts, Glyph[] glyphs) {
      var rects = new RectangleF[glyphs.Length];
      var glyphLayout = new Typography.TextLayout.GlyphLayout();
      var i = 0;
      foreach (var glyph in glyphs) {
        var scale = glyph.Typeface.CalculateScaleToPixelFromPointSize(fonts.PointSize);
        var bounds = glyph.Info.Bounds;
        glyphLayout.Typeface = glyph.Typeface;
        rects[i] = RectangleF.FromLTRB(bounds.XMin * scale, bounds.YMin * scale, bounds.XMax * scale, bounds.YMax * scale);
        i++;
      }
      return rects;
    }

    public float GetTypographicWidth(TFonts fonts, AttributedGlyphRun<TFonts, Glyph> run) => GetAdvancesForGlyphs(fonts, run.Glyphs.ToArray()).Total + run.KernedGlyphs.Sum(g => g.KernAfterGlyph);
  }
}

