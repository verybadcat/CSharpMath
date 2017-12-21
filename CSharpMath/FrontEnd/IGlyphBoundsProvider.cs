using CSharpMath.Display.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Display;

namespace CSharpMath.FrontEnd {
  public interface IGlyphBoundsProvider<TFont, TGlyph>
    where TFont: MathFont<TGlyph> {
    double GetTypographicWidth(TFont font, AttributedGlyphRun<TFont, TGlyph> run);
    /// <summary>This should treat the glyphs independently. In other words,
    /// we don't assume they are one after the other; likely use case is considering
    /// different options.</summary>
    RectangleF[] GetBoundingRectsForGlyphs(TFont font, TGlyph[] glyphs, int nVariants);

    float GetAdvancesForGlyphs(TFont font, TGlyph[] glyphs);
  }
}
