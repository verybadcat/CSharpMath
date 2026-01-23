namespace CSharpMath.Core.BackEnd {
  using TGlyph = System.Text.Rune;
  public class TestGlyphFinder : Display.FrontEnd.IGlyphFinder<TestFont, TGlyph> {
    TestGlyphFinder() { }
    public static TestGlyphFinder Instance { get; } = new TestGlyphFinder();
    public TGlyph FindGlyphForCharacterAtIndex(TestFont font, int index, string str) =>
      // GetRuneAt doesn't support reading at low surrogates
      TGlyph.GetRuneAt(str, char.IsLowSurrogate(str[index]) ? index - 1 : index);
    public System.Collections.Generic.IEnumerable<TGlyph> FindGlyphs(TestFont font, string str) =>
      str.EnumerateRunes();
    public bool GlyphIsEmpty(TGlyph glyph) => glyph == new TGlyph();
    public TGlyph EmptyGlyph => new TGlyph();
  }
}