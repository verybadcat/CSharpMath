namespace CSharpMath.FrontEnd {
  ///<summary>For changing a string into glyphs which will appear on the page.</summary>
  public interface IGlyphFinder<TFont, TGlyph> where TFont : Displays.IFont<TGlyph> {
    TGlyph FindGlyphForCharacterAtIndex(TFont font, int index, string str);
    System.Collections.Generic.IEnumerable<TGlyph> FindGlyphs(TFont font, string str);
    TGlyph EmptyGlyph { get; }
    bool GlyphIsEmpty(TGlyph glyph);
  }
}