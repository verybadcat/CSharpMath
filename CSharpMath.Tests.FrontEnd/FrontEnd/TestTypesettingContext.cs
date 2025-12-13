namespace CSharpMath.Tests.FrontEnd;

using TGlyph = System.Text.Rune;

public static class TestTypesettingContexts {
  public static Display.FrontEnd.TypesettingContext<TestFont, TGlyph> Instance { get; } =
    new Display.FrontEnd.TypesettingContext<TestFont, TGlyph>(
      (font, size) => new TestFont(size),
      TestGlyphBoundsProvider.Instance,
      TestGlyphFinder.Instance,
      new CSharpMath.Apple.JsonMathTable<TestFont, TGlyph>(
        TestFontMeasurer.Instance,
        Newtonsoft.Json.Linq.JToken.Parse(Resources.ManifestResources.LatinMath),
        TestGlyphNameProvider.Instance,
        TestGlyphBoundsProvider.Instance
      )
  );
}
