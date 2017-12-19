using CSharpMath.Display.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Display;

namespace CSharpMath.FrontEnd {
  public interface IGlyphBoundsProvider<TMathFont, TGlyph>
    where TMathFont: MathFont<TGlyph> {
    RectangleF GetCombinedBoundingRectForGlyphs(TMathFont font, TGlyph[] glyphs);
    /// <summary>This should treat the glyphs independently. In other words,
    /// we don't assume they are one after the other; likely use case is considering
    /// different options.</summary>
    RectangleF[] GetBoundingRectsForGlyphs(TMathFont font, TGlyph[] glyphs, int nVariants);

    float GetAdvancesForGlyphs(TMathFont font, TGlyph[] glyphs);
  }
}
