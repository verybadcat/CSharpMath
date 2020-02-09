namespace CSharpMath.Rendering {
  public static class TypesettingContext {
    public static FrontEnd.TypesettingContext<Fonts, Glyph> Instance { get; } =
      new FrontEnd.TypesettingContext<Fonts, Glyph>(
         (fonts, size) => new Fonts(fonts, size),
         GlyphBoundsProvider.Instance,
         GlyphFinder.Instance,
         Displays.UnicodeFontChanger.Instance,
         MathTable.Instance
       );
  }
}
