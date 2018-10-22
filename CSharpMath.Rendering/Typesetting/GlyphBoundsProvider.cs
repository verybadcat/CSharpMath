using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using TFonts = CSharpMath.Rendering.Fonts;

namespace CSharpMath.Rendering {
  public class GlyphBoundsProvider : IGlyphBoundsProvider<TFonts, Glyph> {
    private GlyphBoundsProvider() { }
    public static GlyphBoundsProvider Instance { get; } = new GlyphBoundsProvider();

    public (IEnumerable<float> Advances, float Total) GetAdvancesForGlyphs(TFonts font, ForEach<Glyph> glyphs, int nGlyphs) {
      var advances = new List<float>(nGlyphs);
      foreach (var glyph in glyphs)
        advances.Add(glyph.Typeface.GetHAdvanceWidthFromGlyphIndex(glyph.Info.GlyphIndex) *
        glyph.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize));
      return (advances, advances.Sum());
    }

    public IEnumerable<RectangleF> GetBoundingRectsForGlyphs(TFonts font, ForEach<Glyph> glyphs, int nVariants) {
      var rects = new List<RectangleF>(nVariants);
      foreach (var glyph in glyphs) {
        var scale = glyph.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize);
        var bounds = glyph.Info.Bounds;
        rects.Add(RectangleF.FromLTRB(bounds.XMin * scale, bounds.YMin * scale, bounds.XMax * scale, bounds.YMax * scale));
      }
      return rects;
    }

    public float GetTypographicWidth(TFonts fonts, AttributedGlyphRun<TFonts, Glyph> run) =>
      GetAdvancesForGlyphs(fonts, run.Glyphs.AsForEach(), run.KernedGlyphs.Count).Total + run.KernedGlyphs.Sum(g => g.KernAfterGlyph);
  }
}

