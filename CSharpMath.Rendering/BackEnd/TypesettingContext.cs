namespace CSharpMath.Rendering.BackEnd {
  public static class TypesettingContext {
    public static Display.FrontEnd.TypesettingContext<Fonts, Glyph> Instance { get; } =
      new Display.FrontEnd.TypesettingContext<Fonts, Glyph>(
         (fonts, size) => Fonts.Resize(fonts, size),
         GlyphBoundsProvider.Instance,
         GlyphFinder.Instance,
         MathTable.Instance
       );
  }
}
