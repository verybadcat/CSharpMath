namespace CSharpMath.Core.BackEnd {
  /// <summary>A proportionality constant that is applied when
  /// reading from the Json table.</summary>
  public class TestFontMeasurer {
    TestFontMeasurer() { }
    public static TestFontMeasurer Instance { get; } = new TestFontMeasurer();
    public int GetUnitsPerEm(TestFont font) => 1000;
  }
}