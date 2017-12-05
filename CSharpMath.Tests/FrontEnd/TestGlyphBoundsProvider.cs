using CSharpMath.FrontEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpMath.Display.Text;
using System.Drawing;

namespace CSharpMath.Tests.FrontEnd {
  public class TestGlyphBoundsProvider : IGlyphBoundsProvider<MathFont<char>, char> {
    private const float WidthPerCharacterPerFontSize = 0.5f; // "m" and "M" get double width.
    private const float AscentPerFontSize = 0.7f;
    private const float DescentPerFontSize = 0.2f; // all constants were chosen to bear some resemblance to a real font.

    private int GetEffectiveLength(char[] glyphs) {
      string glyphString = new string(glyphs);
      int length = glyphs.Length;
      int extraLength = glyphString.ToLower().ToArray().Count(x => x == 'm');
      int effectiveLength = length + extraLength;
      return effectiveLength;
    }
    public RectangleF GetBoundingRectForGlyphs(MathFont<char> font, char[] glyphs) {
      int effectiveLength = GetEffectiveLength(glyphs);
      float width = font.PointSize * effectiveLength * WidthPerCharacterPerFontSize;
      float ascent = font.PointSize * AscentPerFontSize;
      float descent = font.PointSize * DescentPerFontSize;
      //  The y axis is NOT inverted. So our y coordinate is minus the descent, i.e. the rect bottom is the descent below the axis.
      return new RectangleF(0, -descent, width, ascent + descent);
    }

    public float GetAdvancesForGlyphs(MathFont<char> font, char[] glyphs) {
      int effectiveLength = GetEffectiveLength(glyphs);
      float width = font.PointSize * effectiveLength * WidthPerCharacterPerFontSize;
      return width;
    }
  }
}
