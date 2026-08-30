namespace CSharpMath.Display.FrontEnd {
  ///<summary>For changing a string into glyphs which will appear on the page.</summary>
  public interface IGlyphFinder<TFont, TGlyph> where TFont : IFont<TGlyph> {
    TGlyph FindGlyphForCharacterAtIndex(TFont font, int index, string str);
    /// <summary>Returns exactly one glyph for each Unicode scalar value in <paramref name="str"/>.</summary>
    /// <remarks>Contextual shaping is performed by text rendering; math glyph lookup is scalar-aligned.</remarks>
    System.Collections.Generic.IEnumerable<TGlyph> FindGlyphs(TFont font, string str);
    TGlyph EmptyGlyph { get; }
    bool GlyphIsEmpty(TGlyph glyph);
  }
}
