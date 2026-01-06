namespace CSharpMath.Core.BackEnd {
  using CSharpMath.Atom;
  using TGlyph = System.Text.Rune;
  public static class TestTypesettingContext {
    public static Result<Display.Displays.ListDisplay<TestFont, System.Text.Rune>>
      CreateDisplay(string latex) =>
      LaTeXParser.MathListFromLaTeX(latex).Bind(mathList =>
        Display.Typesetter.CreateLine(
          mathList, new TestFont(20), TestTypesettingContext.Instance, LineStyle.Display));
    public static Display.FrontEnd.TypesettingContext<TestFont, TGlyph> Instance { get; } =
      new Display.FrontEnd.TypesettingContext<TestFont, TGlyph>(
        (font, size) => new TestFont(size),
        TestGlyphBoundsProvider.Instance,
        TestGlyphFinder.Instance,
        new JsonMathTable(
          TestFontMeasurer.Instance,
          ManifestResources.LatinMath,
          TestGlyphNameProvider.Instance,
          TestGlyphBoundsProvider.Instance
        )
    );
  }
}