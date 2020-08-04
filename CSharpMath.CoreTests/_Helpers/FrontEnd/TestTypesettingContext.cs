namespace CSharpMath.CoreTests.FrontEnd {
  public static class TestTypesettingContexts {
    public static Display.FrontEnd.TypesettingContext<TestFont, char> Instance { get; } =
      new Display.FrontEnd.TypesettingContext<TestFont, char>(
        (font, size) => new TestFont(size),
        TestGlyphBoundsProvider.Instance,
        TestGlyphFinder.Instance,
        new Apple.JsonMathTable<TestFont, char>(
          TestFontMeasurer.Instance,
          Resources.ManifestResources.LatinMath,
          TestGlyphNameProvider.Instance,
          TestGlyphBoundsProvider.Instance
        )
    );
  }
}
