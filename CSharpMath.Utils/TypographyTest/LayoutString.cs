namespace CSharpMath.DevUtils.TypographyTest {
  static class LayoutString {
    public static System.Collections.Generic.IEnumerable<(short x, short y, short w, ushort i)> Layout(string text) {
      var layout = _Statics.GlyphLayout;
      layout.Layout(text.ToCharArray(), 0, text.Length);
      for (int i = 0; i < layout._glyphPositions.Count; i++) {
        var pos = layout._glyphPositions[i];
        yield return (pos.OffsetX, pos.OffsetY, pos.advanceW, pos.glyphIndex);
      }
    }
  }
}
