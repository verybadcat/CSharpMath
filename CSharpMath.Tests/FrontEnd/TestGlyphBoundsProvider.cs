using CSharpMath.FrontEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpMath.Display;
using System.Drawing;
using TGlyph = System.Char;
using CSharpMath.Display.Text;

namespace CSharpMath.Tests.FrontEnd {
  public class TestGlyphBoundsProvider : IGlyphBoundsProvider<TestMathFont, TGlyph> {
    private const float WidthPerCharacterPerFontSize = 0.5f; // "m" and "M" get double width.
    private const float AscentPerFontSize = 0.7f;
    private const float DescentPerFontSize = 0.2f; // all constants were chosen to bear some resemblance to a real font.

    private int GetEffectiveLength(IEnumerable<TGlyph> enumerable) {
      int length = 0;
      foreach (var c in enumerable)
        if (c is 'M' || c is 'm') length += 2;
        else length++;
      return length;
    }

    private int GetEffectiveLength(ReadOnlySpan<TGlyph> span) {
      int length = 0;
      for(int i = 0; i < span.Length; i++)
        if (span[i] is 'M' || span[i] is 'm') length += 2;
        else length++;
      return length;
    }

    public float GetTypographicWidth(TestMathFont font, AttributedGlyphRun<TestMathFont, TGlyph> run) {
      int effectiveLength = GetEffectiveLength(run.Glyphs);
      float width = font.PointSize * effectiveLength * WidthPerCharacterPerFontSize +
                         run.KernedGlyphs.Sum(g => g.KernAfterGlyph);
      return width;
    }

    public IEnumerable<RectangleF> GetBoundingRectsForGlyphs(TestMathFont font, IEnumerable<TGlyph> glyphs, int nGlyphs) {
      RectangleF[] r = new RectangleF[glyphs.Length];
      foreach(var glyph in glyphs) {
        int effectiveLength = GetEffectiveLength(stackalloc[] { glyph });
        float width = font.PointSize * effectiveLength * WidthPerCharacterPerFontSize;
        float ascent = font.PointSize * AscentPerFontSize;
        float descent = font.PointSize * DescentPerFontSize;
        //  The y axis is NOT inverted. So our y coordinate is minus the descent, i.e. the rect bottom is the descent below the axis.
        r[i] = new RectangleF(0, -descent, width, ascent + descent);
      }
      return r;
    }

    public (float[] Advances, float Total) GetAdvancesForGlyphs(TestMathFont font, TGlyph[] glyphs) {
      var r = new float[glyphs.Length];
      var total = 0f;
      for (int i = 0; i < glyphs.Length; i++) {
        total += r[i] = GetEffectiveLength(glyphs[i]) * font.PointSize * WidthPerCharacterPerFontSize;
      }
      return (r, total);
    }
  }
}
