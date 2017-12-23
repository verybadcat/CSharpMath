using System;
using System.Collections.Generic;
using CSharpMath.Atoms;
using System.Diagnostics;
using System.Linq;

namespace CSharpMath.Display.Text {
  /// <summary>Like an attributed string, but the attributes other than Kern are required to be fixed
  /// over the whole string. We use KernedGlyph objects instead of Glyphs to
  /// allow us to set kern on a per-glyph basis.</summary>
  public class AttributedGlyphRun<TFont, TGlyph>
    where TFont: MathFont<TGlyph>{
    public TGlyph[] Glyphs => KernedGlyphs.Select(g => g.Glyph).ToArray();
    public KernedGlyph<TGlyph>[] KernedGlyphs { get; set; }
    public string Text { get; set; }

    public int Length => KernedGlyphs.Length;
    public TFont Font { get; set; }
    public MathColor TextColor { get; set; }

    public override string ToString() => $"AttributedGlyphRun {KernedGlyphs.Length} glyphs";

    public AttributedGlyphRun() {
    }
  }

 
  public static class AttributedGlyphRunExtensions {
    public static bool AttributesMatch<TMathFont, TGlyph>(this AttributedGlyphRun<TMathFont, TGlyph> run1, AttributedGlyphRun<TMathFont, TGlyph> run2)
      where TMathFont: MathFont<TGlyph> {
      if (run1==null || run2 == null) {
        return false;
      }
      if (!AnyType.SafeEquals(run1.Font, run2.Font)) {
        return false;
      }
      if (!AnyType.SafeEquals(run1.TextColor, run2.TextColor)) {
        return false;
      }
      return true;
    }
  }
}
