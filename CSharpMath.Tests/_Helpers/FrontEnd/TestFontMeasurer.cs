namespace CSharpMath.Tests.FrontEnd {
  class TestFontMeasurer : Displays.FrontEnd.IFontMeasurer<TestFont, char> {
    TestFontMeasurer() { }
    public static TestFontMeasurer Instance { get; } = new TestFontMeasurer();
    public int GetUnitsPerEm(TestFont font) => 1000;
  }
}
