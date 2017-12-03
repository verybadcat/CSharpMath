using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Display.Text {
  /// <summary>Like an attributed string, but the attributes are required to be fixed
  /// over the whole string.</summary>
  public class AttributedGlyphRun<TGlyph> {
    public TGlyph[] Text { get; set; }
    public int Length => Text.Length;
    public MathFont Font { get; set; }
    public Color TextColor { get; set; }
    public float Kern { get; set; }

    public override string ToString() => "AttributedGlyphRun " + Text;

  }
 
  public static class AttributedGlyphRunExtensions {
    public static bool AttributesMatch<TGlyph>(this AttributedGlyphRun<TGlyph> run1, AttributedGlyphRun<TGlyph> run2) {
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
