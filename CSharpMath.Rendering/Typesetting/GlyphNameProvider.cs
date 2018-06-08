using Typography.OpenFont;
namespace CSharpMath.Rendering {
  public class GlyphNameProvider: IGlyphNameProvider<Glyph>
  {
    private readonly Typeface _typeface;

    public GlyphNameProvider(Typeface typeface) => _typeface = typeface;

    public Glyph GetGlyph(string glyphName) => _typeface.GetGlyphByName(glyphName);

    public string GetGlyphName(Glyph glyph) => glyph.GetCff1GlyphData().Name;
  }
}
