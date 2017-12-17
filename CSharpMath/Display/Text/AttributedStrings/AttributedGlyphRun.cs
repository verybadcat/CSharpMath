using System;
using System.Collections.Generic;
using CSharpMath.Atoms;
using System.Diagnostics;

namespace CSharpMath.Display.Text {
  /// <summary>Like an attributed string, but the attributes are required to be fixed
  /// over the whole string.</summary>
  public class AttributedGlyphRun<TMathFont, TGlyph>
    where TMathFont: MathFont<TGlyph>{
    public TGlyph[] Glyphs { get; set; }
    public string Text { get; set; }

    public int Length => Glyphs.Length;
    public TMathFont Font { get; set; }
    public MathColor TextColor { get; set; }
    public float Kern { get; set; }

    public override string ToString() => "AttributedGlyphRun " + Glyphs;



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
      if (!(Math.Abs(run1.Kern - run2.Kern) < 1E-4f)) { // allow a little roundoff error
        return false;
      }
      return true;
    }
  }
}
