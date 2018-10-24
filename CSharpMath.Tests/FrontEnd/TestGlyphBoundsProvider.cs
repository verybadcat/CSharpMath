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

    public float GetTypographicWidth(TestMathFont font, AttributedGlyphRun<TestMathFont, TGlyph> run) =>
      font.PointSize * GetEffectiveLength(run.Glyphs) * WidthPerCharacterPerFontSize + run.GlyphInfos.Sum(g => g.KernAfterGlyph);

    public IEnumerable<RectangleF> GetBoundingRectsForGlyphs(TestMathFont font, ForEach<TGlyph> glyphs, int nGlyphs) =>
      ForEach<TGlyph>.AllocateNewArrayFor(glyphs).Select(glyph => {
        ReadOnlySpan<TGlyph> span = stackalloc[] { glyph };
        float width = font.PointSize * GetEffectiveLength(span) * WidthPerCharacterPerFontSize;
        float ascent = font.PointSize * AscentPerFontSize;
        float descent = font.PointSize * DescentPerFontSize;
        //  The y axis is NOT inverted. So our y coordinate is minus the descent, i.e. the rect bottom is the descent below the axis.
        return new RectangleF(0, -descent, width, ascent + descent);
      });

    public (IEnumerable<float> Advances, float Total) GetAdvancesForGlyphs(TestMathFont font, ForEach<TGlyph> glyphs, int nGlyphs) {
      var r = new float[nGlyphs];
      var total = 0f;
      int i = 0;
      foreach(var glyph in glyphs) {
        ReadOnlySpan<TGlyph> span = stackalloc[] { glyph };
        total += r[i] = GetEffectiveLength(span) * font.PointSize * WidthPerCharacterPerFontSize;
        i++;
      }
      return (r, total);
    }
  }
}
