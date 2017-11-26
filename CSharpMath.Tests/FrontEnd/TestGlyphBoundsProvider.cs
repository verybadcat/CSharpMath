using CSharpMath.FrontEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpMath.Display.Text;
using System.Drawing;

namespace CSharpMath.Tests.FrontEnd {
  public class TestGlyphBoundsProvider : IGlyphBoundsProvider {
    private const float WidthPerCharacterPerFontSize = 0.1f; // "m" and "M" get double width.
    private const float AscentPerFontSize = 0.2f;
    private const float DescentPerFontSize = 0.05f;
    public RectangleF GetBoundingRectForGlyphs(MathFont font, string glyphs) {
      float pointSize = font.PointSize;
      int length = glyphs.Length;
      int extraLength = glyphs.ToLower().ToArray().Count(x => x == 'm');
      int effectiveLength = length + extraLength;
      float width = font.PointSize * effectiveLength * WidthPerCharacterPerFontSize;
      float ascent = font.PointSize * AscentPerFontSize;
      float descent = font.PointSize * DescentPerFontSize;
      return new RectangleF(0, -ascent, width, ascent + descent);
    }
  }
}
