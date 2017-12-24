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
    /// <summary>The last float in the return value is the overall advance. The remainder
    /// gives the advances for the individual glyphs. The length of the returned array
    /// is therefore one greater than the number of glyphs.</summary>
    float[] GetAdvancesForGlyphs(TFont font, TGlyph[] glyphs);
  }
}
