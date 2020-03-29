namespace CSharpMath.CoreTests.FrontEnd {
  class TestFontMeasurer : Display.FrontEnd.IFontMeasurer<TestFont, char> {
    TestFontMeasurer() { }
    public static TestFontMeasurer Instance { get; } = new TestFontMeasurer();
    public int GetUnitsPerEm(TestFont font) => 1000;
  }
}
