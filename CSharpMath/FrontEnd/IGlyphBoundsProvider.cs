using CSharpMath.Display.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.FrontEnd {
  public interface IGlyphBoundsProvider<TGlyph> {
    RectangleF GetBoundingRectForGlyphs(MathFont<TGlyph> font, TGlyph[] glyphs);
    float GetAdvancesForGlyphs(MathFont<TGlyph> font, TGlyph[] glyphs);
  }
}
