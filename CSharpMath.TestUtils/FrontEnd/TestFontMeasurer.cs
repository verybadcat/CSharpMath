namespace CSharpMath.TestUtils;

using TGlyph = System.Text.Rune;

public class TestFontMeasurer : IFontMeasurer<TestFont, TGlyph> {
  TestFontMeasurer() { }
  public static TestFontMeasurer Instance { get; } = new TestFontMeasurer();
  public int GetUnitsPerEm(TestFont font) => 1000;
}
