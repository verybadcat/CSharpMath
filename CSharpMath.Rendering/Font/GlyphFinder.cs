using System.Linq;
using CSharpMath.FrontEnd;

namespace CSharpMath.Rendering {
  public class GlyphFinder : IGlyphFinder<Glyph> {
    private readonly MathFonts _fonts;

    public GlyphFinder(MathFonts fontSizeIrrelevant) => _fonts = fontSizeIrrelevant;

    Glyph Lookup(int codepoint) {
      foreach (var font in _fonts) {
        var g = font.Lookup(codepoint);
        if (g.GlyphIndex != 0) return new Glyph(font, g);
      }
      return Glyph.Empty;
    }

    public Glyph FindGlyphForCharacterAtIndex(int index, string str) {
      var codepoint = char.ConvertToUtf32(str, index - (char.IsLowSurrogate(str[index]) ? 1 : 0));
      return Lookup(codepoint);
    }

    public Glyph[] FindGlyphs(string str) => Typography.OpenFont.StringUtils.GetCodepoints(str.ToCharArray()).Select(Lookup).ToArray();

    public bool GlyphIsEmpty(Glyph glyph) => glyph.IsEmpty;
  }
}
