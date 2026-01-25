namespace CSharpMath.Core.BackEnd {
  using System.Linq;
  using TGlyph = System.Text.Rune;
  /// <summary>Looks up a name in latinmodern-math.json. The names provided by this class are used to lookup spacings in JsonMathTable.cs.</summary>
  public class TestGlyphNameProvider {
    TestGlyphNameProvider() { }
    public static TestGlyphNameProvider Instance { get; } = new TestGlyphNameProvider();
    static readonly TGlyph italic_a = "𝑎".EnumerateRunes().Single();
    static readonly TGlyph italic_A = "𝐴".EnumerateRunes().Single();
    static readonly TGlyph italic_z = "𝑧".EnumerateRunes().Single();
    static readonly TGlyph italic_Z = "𝑍".EnumerateRunes().Single();
    public string GetGlyphName(TGlyph glyph) =>
      // We don't want to have complicated code for custom names in latinmodern-math.json at this moment
      glyph >= italic_a && glyph <= italic_z
      ? ((char)(glyph.Value - italic_a.Value + 'a')).ToString()
      : glyph >= italic_A && glyph <= italic_Z
      ? ((char)(glyph.Value - italic_A.Value + 'A')).ToString()
      : glyph.IsBmp
      ? $"uni{glyph.Value.ToStringInvariant("X4")}"
      : $"u{glyph.Value.ToStringInvariant("X")}";
    public TGlyph GetGlyph(string glyphName) {
      // Variant glyphs have a dot in their name
      var actualName = glyphName.IndexOf('.') switch { -1 => glyphName, var dotIndex => glyphName.Substring(0, dotIndex) };
      return actualName.Length switch
      {
        1 when actualName[0] >= 'a' && actualName[0] <= 'z' => new TGlyph(actualName[0] - 'a' + italic_a.Value),
        1 when actualName[0] >= 'A' && actualName[0] <= 'Z' => new TGlyph(actualName[0] - 'A' + italic_A.Value),
        5 => new TGlyph(int.Parse(actualName.Substring(1), System.Globalization.NumberStyles.AllowHexSpecifier)),
        7 => new TGlyph(int.Parse(actualName.Substring(3), System.Globalization.NumberStyles.AllowHexSpecifier)),
        _ => throw new System.Runtime.CompilerServices.SwitchExpressionException((object)actualName)
      };
    }
  }
}