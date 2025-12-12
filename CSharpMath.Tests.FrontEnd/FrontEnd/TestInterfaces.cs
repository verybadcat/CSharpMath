namespace CSharpMath.Tests.FrontEnd;

using System.Text;

/// <remarks>
/// The names provided by this class are used to lookup spacings in JsonMathTable.cs.
/// </remarks>
public interface IGlyphNameProvider {
  string GetGlyphName(Rune glyph);
  Rune GetGlyph(string glyphName);
}

public interface IFontMeasurer {
  /// <summary>A proportionality constant that is applied when
  /// reading from the Json table.</summary>
  int GetUnitsPerEm(TestFont font);
}
