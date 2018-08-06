using CSharpMath.FrontEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpMath.Display;
using System.Drawing;
using TGlyph = System.Char;
using TFont = CSharpMath.Display.MathFont<System.Char>;
using CSharpMath.Display.Text;

namespace CSharpMath.Tests.FrontEnd {
  public class TestGlyphBoundsProvider : IGlyphBoundsProvider<MathFont<TGlyph>, TGlyph> {
    private const float WidthPerCharacterPerFontSize = 0.5f; // "m" and "M" get double width.
    private const float AscentPerFontSize = 0.7f;
    private const float DescentPerFontSize = 0.2f; // all constants were chosen to bear some resemblance to a real font.

    private int GetEffectiveLength(TGlyph glyph)
    => GetEffectiveLength(new TGlyph[] { glyph });

    private int GetEffectiveLength(TGlyph[] glyphs) {
      string glyphString = new string(glyphs);
      int length = glyphs.Length;
      int extraLength = glyphString.ToLower().ToArray().Count(x => x == 'm');
      int effectiveLength = length + extraLength;
      return effectiveLength;
    }

    public float GetTypographicWidth(MathFont<char> font, AttributedGlyphRun<TFont, TGlyph> run) {
      int effectiveLength = GetEffectiveLength(run.Glyphs.ToArray());
      float width = font.PointSize * effectiveLength * WidthPerCharacterPerFontSize +
                         run.KernedGlyphs.Sum(g => g.KernAfterGlyph);
      return width;
    }

    public RectangleF[] GetBoundingRectsForGlyphs(TFont font, TGlyph[] glyphs) {
      RectangleF[] r = new RectangleF[glyphs.Length];
      for (int i = 0; i < glyphs.Length; i++) {
        var glyph = glyphs[i];
        TGlyph[] singleGlyph = { glyph };
        int effectiveLength = GetEffectiveLength(singleGlyph);
        float width = font.PointSize * effectiveLength * WidthPerCharacterPerFontSize;
        float ascent = font.PointSize * AscentPerFontSize;
        float descent = font.PointSize * DescentPerFontSize;
        //  The y axis is NOT inverted. So our y coordinate is minus the descent, i.e. the rect bottom is the descent below the axis.
        r[i] = new RectangleF(0, -descent, width, ascent + descent);
      }
      return r;
    }

    public (float[] Advances, float Total) GetAdvancesForGlyphs(MathFont<TGlyph> font, TGlyph[] glyphs) {
      var r = new float[glyphs.Length];
      var total = 0f;
      for (int i = 0; i < glyphs.Length; i++) {
        total += r[i] = GetEffectiveLength(glyphs[i]) * font.PointSize * WidthPerCharacterPerFontSize;
      }
      return (r, total);
    }
  }
}
