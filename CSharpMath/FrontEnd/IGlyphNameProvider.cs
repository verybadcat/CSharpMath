namespace CSharpMath {
  /// <remarks>The names provided by this class are used to lookup spacings in MathTable.cs.</remarks>
  public interface IGlyphNameProvider<TGlyph> {
    string GetGlyphName(TGlyph glyph);
    TGlyph GetGlyph(string glyphName);
  }
}
