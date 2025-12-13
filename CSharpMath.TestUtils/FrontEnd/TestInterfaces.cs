namespace CSharpMath.TestUtils;

using System.Text;

/// <remarks>
/// The names provided by this class are used to lookup spacings in JsonMathTable.cs.
/// </remarks>
public interface IGlyphNameProvider<TGlyph> {
  string GetGlyphName(TGlyph glyph);
  TGlyph GetGlyph(string glyphName);
}

public interface IFontMeasurer<TFont, TGlyph> {
  /// <summary>A proportionality constant that is applied when
  /// reading from the Json table.</summary>
  int GetUnitsPerEm(TFont font);
}
