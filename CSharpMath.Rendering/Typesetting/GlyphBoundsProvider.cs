using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using TFonts = CSharpMath.Rendering.Fonts;

namespace CSharpMath.Rendering {
  public class GlyphBoundsProvider : IGlyphBoundsProvider<TFonts, Glyph> {
    private GlyphBoundsProvider() { }
    private static Typography.TextLayout.GlyphLayout GlyphLayout = new Typography.TextLayout.GlyphLayout();
    public static GlyphBoundsProvider Instance { get; } = new GlyphBoundsProvider();

    public (IEnumerable<float> Advances, float Total) GetAdvancesForGlyphs(TFonts font, IEnumerable<Glyph> glyphs, int nGlyphs) {
      var advances = glyphs.Select(glyph =>
        glyph.Typeface.GetHAdvanceWidthFromGlyphIndex(glyph.Info.GlyphIndex) *
        glyph.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize)).ToList();
      return (advances, advances.Sum());
    }

    public IEnumerable<RectangleF> GetBoundingRectsForGlyphs(TFonts font, IEnumerable<Glyph> glyphs, int nVariants) {
      var rects = System.Buffers.ArrayPool<RectangleF>.Shared.Rent(nVariants);
      var i = 0;
      foreach (var glyph in glyphs) {
        var scale = glyph.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize);
        var bounds = glyph.Info.Bounds;
        GlyphLayout.Typeface = glyph.Typeface;
        rects[i] = RectangleF.FromLTRB(bounds.XMin * scale, bounds.YMin * scale, bounds.XMax * scale, bounds.YMax * scale);
        i++;
      }
      return rects;
    }

    public float GetTypographicWidth(TFonts fonts, AttributedGlyphRun<TFonts, Glyph> run) => GetAdvancesForGlyphs(fonts, run.Glyphs, run.KernedGlyphs.Count).Total + run.KernedGlyphs.Sum(g => g.KernAfterGlyph);
  }
}

