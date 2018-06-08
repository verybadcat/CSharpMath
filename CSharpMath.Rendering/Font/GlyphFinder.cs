using System.Linq;
using CSharpMath.FrontEnd;
using Typography.OpenFont;

namespace CSharpMath.Rendering {
  public class GlyphFinder : IGlyphFinder<Glyph> {
    private readonly Typeface _typeface;

    public GlyphFinder(Typeface typeface) => _typeface = typeface;

    public Glyph FindGlyphForCharacterAtIndex(int index, string str) =>
      _typeface.Lookup(char.ConvertToUtf32(str, index - (char.IsLowSurrogate(str[index]) ? 1 : 0)));

    public Glyph[] FindGlyphs(string str) => StringUtils.GetCodepoints(str.ToCharArray()).Select(_typeface.Lookup).ToArray();

    public bool GlyphIsEmpty(Glyph glyph) => glyph == null || glyph == Glyph.Empty;
  }
}
