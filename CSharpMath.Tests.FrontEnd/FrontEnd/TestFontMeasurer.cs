namespace CSharpMath.Tests.FrontEnd;

class TestFontMeasurer : IFontMeasurer {
  TestFontMeasurer() { }
  public static TestFontMeasurer Instance { get; } = new TestFontMeasurer();
  public int GetUnitsPerEm(TestFont font) => 1000;
}
