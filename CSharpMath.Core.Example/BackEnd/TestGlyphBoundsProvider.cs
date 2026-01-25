using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TGlyph = System.Text.Rune;
using CSharpMath.Display;

namespace CSharpMath.Core.BackEnd {
  public class TestGlyphBoundsProvider : Display.FrontEnd.IGlyphBoundsProvider<TestFont, TGlyph> {
    // all constants were chosen to bear some resemblance to a real font.
    private const float WidthPerCharacterPerFontSize = 0.5f; // "m" and "M" get double width.
    private const float AscentPerFontSize = 0.7f;
    private const float DescentPerFontSize = 0.2f;

    TestGlyphBoundsProvider() { }
    public static TestGlyphBoundsProvider Instance { get; } = new TestGlyphBoundsProvider();

    static readonly HashSet<TGlyph> M =
      new HashSet<TGlyph> { new TGlyph('M'), new TGlyph('m'), TGlyph.GetRuneAt("𝑀", 0), TGlyph.GetRuneAt("𝑚", 0), };
    private int GetEffectiveLength(IEnumerable<TGlyph> enumerable) {
      int length = 0;
      foreach (var c in enumerable)
        if (M.Contains(c)) length += 2;
        else length++;
      return length;
    }

    private int GetEffectiveLength(ReadOnlySpan<TGlyph> span) {
      int length = 0;
      for(int i = 0; i < span.Length; i++)
        if (M.Contains(span[i])) length += 2;
        else length++;
      return length;
    }

    public float GetTypographicWidth(TestFont font, AttributedGlyphRun<TestFont, TGlyph> run) =>
      font.PointSize * GetEffectiveLength(run.Glyphs) * WidthPerCharacterPerFontSize
      + run.GlyphInfos.Sum(g => g.KernAfterGlyph);

    public IEnumerable<RectangleF> GetBoundingRectsForGlyphs
      (TestFont font, IEnumerable<TGlyph> glyphs, int nGlyphs) =>
      glyphs.Select(glyph => {
        ReadOnlySpan<TGlyph> span = stackalloc[] { glyph };
        float width = font.PointSize * GetEffectiveLength(span) * WidthPerCharacterPerFontSize;
        float ascent = font.PointSize * AscentPerFontSize;
        float descent = font.PointSize * DescentPerFontSize;
        // The y axis is NOT inverted. So our y coordinate is minus the descent,
        // i.e. the rect bottom is the descent below the axis.
        return new RectangleF(0, -descent, width, ascent + descent);
      });

    public (IEnumerable<float> Advances, float Total) GetAdvancesForGlyphs
      (TestFont font, IEnumerable<TGlyph> glyphs, int nGlyphs) {
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
