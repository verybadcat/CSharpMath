namespace CSharpMath.Tests.FrontEnd {
  class TestGlyphNameProvider : Displays.FrontEnd.IGlyphNameProvider<char> {
    TestGlyphNameProvider() { }
    public static TestGlyphNameProvider Instance { get; } = new TestGlyphNameProvider();
    public char GetGlyph(string glyphName) => System.Linq.Enumerable.FirstOrDefault(glyphName);
    public string GetGlyphName(char glyph) => glyph.ToString();
  }
}
