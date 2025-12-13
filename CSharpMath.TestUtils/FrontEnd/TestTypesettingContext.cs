namespace CSharpMath.TestUtils;

using TGlyph = System.Text.Rune;

public static class TestTypesettingContexts {
  public static Display.FrontEnd.TypesettingContext<TestFont, TGlyph> Instance { get; } =
    new Display.FrontEnd.TypesettingContext<TestFont, TGlyph>(
      (font, size) => new TestFont(size),
      TestGlyphBoundsProvider.Instance,
      TestGlyphFinder.Instance,
      new JsonMathTable<TestFont, TGlyph>(
        TestFontMeasurer.Instance,
        Resources.ManifestResources.LatinMath,
        TestGlyphNameProvider.Instance,
        TestGlyphBoundsProvider.Instance
      )
  );
}
