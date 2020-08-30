namespace CSharpMath.CoreTests.FrontEnd {
  using TGlyph = System.Text.Rune;
  class TestGlyphNameProvider : Display.FrontEnd.IGlyphNameProvider<TGlyph> {
    TestGlyphNameProvider() { }
    public static TestGlyphNameProvider Instance { get; } = new TestGlyphNameProvider();
    public TGlyph GetGlyph(string glyphName) => TGlyph.GetRuneAt(glyphName, 0);
    public string GetGlyphName(TGlyph glyph) => glyph.ToString();
  }
}