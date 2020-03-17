using System.Linq;

namespace CSharpMath.Rendering.BackEnd {
  public class GlyphFinder : Display.FrontEnd.IGlyphFinder<Fonts, Glyph> {
    private GlyphFinder() { }
    //http://unicode.org/charts/PDF/U25A0.pdf
    //U+25A1 WHITE SQUARE may be used to represent a missing ideograph
    //The glyph of this character is in the Latin Modern Math font
    public const char GlyphNotFound = '□';
    public static GlyphFinder Instance { get; } = new GlyphFinder();
    public Glyph Lookup(Fonts fonts, int codepoint) {
      foreach (var font in fonts) {
        var g = font.GetGlyphIndex(codepoint);
        if (g != 0) return new Glyph(font, font.GetGlyph(g));
      }
      return Lookup(fonts, GlyphNotFound);
    }
    public int GetCodepoint(string str, int index) =>
      index + 1 < str.Length
      && char.IsHighSurrogate(str[index])
      && char.IsLowSurrogate(str[index + 1])
      ? char.ConvertToUtf32(str[index], str[index + 1])
      : index > 0
      && char.IsHighSurrogate(str[index - 1])
      && char.IsLowSurrogate(str[index])
      ? char.ConvertToUtf32(str[index - 1], str[index])
      : str[index];
    public Glyph FindGlyphForCharacterAtIndex(Fonts fonts, int index, string str) =>
      Lookup(fonts, GetCodepoint(str, index));
    public System.Collections.Generic.IEnumerable<Glyph> FindGlyphs(Fonts fonts, string str) =>
      Typography.OpenFont.StringUtils.GetCodepoints(str.ToCharArray())
      .Select(c => Lookup(fonts, c));
    public bool GlyphIsEmpty(Glyph glyph) => glyph.IsEmpty;
    public Glyph EmptyGlyph => Glyph.Empty;
  }
}
