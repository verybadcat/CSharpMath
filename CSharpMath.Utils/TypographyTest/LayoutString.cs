namespace CSharpMath.DevUtils.TypographyTest {
  static class LayoutString {
    public static System.Collections.Generic.IEnumerable<(short x, short y, short w, ushort i)> Layout(string text) {
      var layout = _Statics.GlyphLayout;
      layout.Layout(text.ToCharArray(), 0, text.Length);
      for (int i = 0; i < layout.ResultUnscaledGlyphPositions.Count; i++) {
        var glyph = layout.ResultUnscaledGlyphPositions.GetGlyph(i, out _, out var x, out var y, out var w);
        yield return (x, y, w, glyph);
      }
    }
  }
}
