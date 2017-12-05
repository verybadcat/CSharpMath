using CSharpMath.Display.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.FrontEnd {
  public interface IGlyphBoundsProvider<TMathFont, TGlyph>
    where TMathFont: MathFont<TGlyph> {
    RectangleF GetBoundingRectForGlyphs(TMathFont font, TGlyph[] glyphs);
    float GetAdvancesForGlyphs(TMathFont font, TGlyph[] glyphs);
  }
}
