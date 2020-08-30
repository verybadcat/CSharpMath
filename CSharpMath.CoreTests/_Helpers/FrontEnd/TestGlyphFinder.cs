namespace CSharpMath.CoreTests.FrontEnd {
  using TGlyph = System.Text.Rune;
  class TestGlyphFinder : Display.FrontEnd.IGlyphFinder<TestFont, TGlyph> {
    TestGlyphFinder() { }
    public static TestGlyphFinder Instance { get; } = new TestGlyphFinder();
    public TGlyph FindGlyphForCharacterAtIndex
      (TestFont font, int index, string str) => TGlyph.GetRuneAt(str, index);
    public System.Collections.Generic.IEnumerable<TGlyph> FindGlyphs
     (TestFont font, string str) => str.EnumerateRunes();
    public bool GlyphIsEmpty(TGlyph glyph) => glyph == new TGlyph();
    public TGlyph EmptyGlyph => new TGlyph();
  }
}