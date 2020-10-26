using System;

namespace CSharpMath.Display.FrontEnd {
  /// <summary>
  /// A wrapper class holding everything the core needs to have in order to layout the LaTeX.
  /// </summary>
  public class TypesettingContext<TFont, TGlyph> where TFont: IFont<TGlyph> {
    public IGlyphBoundsProvider<TFont, TGlyph> GlyphBoundsProvider { get; }
    public IGlyphFinder<TFont, TGlyph> GlyphFinder { get; }
    public FontMathTable<TFont, TGlyph> MathTable { get; }
    public Func<TFont, float, TFont> MathFontCloner { get; }
    public TypesettingContext(Func<TFont, float, TFont> mathFontCloner,
      IGlyphBoundsProvider<TFont, TGlyph> glyphBoundsProvider,
      IGlyphFinder<TFont, TGlyph> glyphFinder,
      FontMathTable<TFont, TGlyph> mathTable) {
      GlyphBoundsProvider = glyphBoundsProvider;
      MathFontCloner = mathFontCloner;
      GlyphFinder = glyphFinder;
      MathTable = mathTable;
    }
  }
}