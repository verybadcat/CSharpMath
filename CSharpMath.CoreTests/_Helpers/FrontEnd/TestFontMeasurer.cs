namespace CSharpMath.CoreTests.FrontEnd {
  class TestFontMeasurer : CSharpMath.Editor.Tests.IFontMeasurer {
    TestFontMeasurer() { }
    public static TestFontMeasurer Instance { get; } = new TestFontMeasurer();
    public int GetUnitsPerEm(TestFont font) => 1000;
  }
}
