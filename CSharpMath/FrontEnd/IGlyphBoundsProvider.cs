using CSharpMath.Display.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.FrontEnd {
  public interface IGlyphBoundsProvider {
    RectangleF GetBoundingRectForGlyphs(MathFont font, string glyphs);
  }
}
