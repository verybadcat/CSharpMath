using CSharpMath.Display.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.FrontEnd {
  public interface IGlyphBoundsProvider<TGlyph> {
    RectangleF GetBoundingRectForGlyphs(MathFont font, TGlyph[] glyphs);
    float GetAdvancesForGlyphs(MathFont font, TGlyph[] glyphs);
  }
}
