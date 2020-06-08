using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSharpMath.Display;
using CSharpMath.Display.FrontEnd;

namespace CSharpMath.Rendering.BackEnd {
  public class GlyphBoundsProvider : IGlyphBoundsProvider<Fonts, Glyph> {
    private GlyphBoundsProvider() { }
    public static GlyphBoundsProvider Instance { get; } = new GlyphBoundsProvider();
    public (IEnumerable<float> Advances, float Total) GetAdvancesForGlyphs
      (Fonts font, IEnumerable<Glyph> glyphs, int nGlyphs) {
      var advances = new List<float>(nGlyphs);
      var last = Glyph.Empty;
      foreach (var glyph in glyphs) {
        advances.Add(glyph.Typeface.GetHAdvanceWidthFromGlyphIndex(glyph.Info.GlyphIndex) *
          glyph.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize));
        last = glyph;
      }
      return (advances, (nGlyphs <= 1 ? 0 : advances.Take(nGlyphs - 1).Sum()) +
        last.Info.Bounds.XMax * last.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize));
    }
    public IEnumerable<RectangleF> GetBoundingRectsForGlyphs
      (Fonts font, IEnumerable<Glyph> glyphs, int nVariants) {
      var rects = new List<RectangleF>(nVariants);
      foreach (var glyph in glyphs) {
        var scale = glyph.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize);
        var bounds = glyph.Info.Bounds;
        rects.Add(RectangleF.FromLTRB
          (bounds.XMin * scale, bounds.YMin * scale, bounds.XMax * scale, bounds.YMax * scale));
      }
      return rects;
    }
    public float GetTypographicWidth(Fonts fonts, AttributedGlyphRun<Fonts, Glyph> run) =>
      GetAdvancesForGlyphs(fonts, run.Glyphs, run.GlyphInfos.Count).Total
      + run.GlyphInfos.Sum(g => g.KernAfterGlyph);
  }
}

