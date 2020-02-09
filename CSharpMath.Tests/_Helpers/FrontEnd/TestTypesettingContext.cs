namespace CSharpMath.Tests.FrontEnd {
  public static class TestTypesettingContexts {
    public static CSharpMath.FrontEnd.TypesettingContext<TestFont, char> Instance { get; } =
      new CSharpMath.FrontEnd.TypesettingContext<TestFont, char>(
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
