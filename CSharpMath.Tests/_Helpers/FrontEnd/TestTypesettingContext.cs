namespace CSharpMath.Tests.FrontEnd {
  public static class TestTypesettingContexts {
    public static Displays.FrontEnd.TypesettingContext<TestFont, char> Instance { get; } =
      new Displays.FrontEnd.TypesettingContext<TestFont, char>(
        (font, size) => new TestFont(size),
        TestGlyphBoundsProvider.Instance,
        TestGlyphFinder.Instance,
        DoNothingFontChanger.Instance,
        new Apple.JsonMathTable<TestFont, char>(
          TestFontMeasurer.Instance,
          Resources.ManifestResources.LatinMath,
          TestGlyphNameProvider.Instance,
          TestGlyphBoundsProvider.Instance
        )
    );
  }
}
