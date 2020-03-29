namespace CSharpMath.CoreTests.FrontEnd {
  class TestGlyphFinder : Display.FrontEnd.IGlyphFinder<TestFont, char> {
    TestGlyphFinder() { }
    public static TestGlyphFinder Instance { get; } = new TestGlyphFinder();
    public char FindGlyphForCharacterAtIndex
      (TestFont font, int index, string str) => str[index];
    public System.Collections.Generic.IEnumerable<char> FindGlyphs
     (TestFont font, string str) => str;
    public bool GlyphIsEmpty(char glyph) => glyph is '\0';
    public char EmptyGlyph => '\0';
  }
}
