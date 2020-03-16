using System.Collections.Generic;

namespace CSharpMath.Display.FrontEnd {
  public interface IGlyphBoundsProvider<TFont, TGlyph> where TFont: IFont<TGlyph> {
    /// <summary>The width of the glyph run.</summary>
    float GetTypographicWidth(TFont font, AttributedGlyphRun<TFont, TGlyph> run);
    /// <summary>This should treat the glyphs independently.
    /// In other words, we don't assume they are one after the other;
    /// likely use case is considering different options.</summary>
    IEnumerable<System.Drawing.RectangleF> GetBoundingRectsForGlyphs
      (TFont font, IEnumerable<TGlyph> glyphs, int nGlyphs);
    /// <summary>The advances for the individual glyphs and the overall advance.</summary>
    (IEnumerable<float> Advances, float Total) GetAdvancesForGlyphs
      (TFont font, IEnumerable<TGlyph> glyphs, int nGlyphs);
  }
}