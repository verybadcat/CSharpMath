using Typography.OpenFont;
namespace CSharpMath.SkiaSharp
{
  public class SkiaGlyphNameProvider: IGlyphNameProvider<Glyph>
  {
    private readonly Typeface _typeface;

    public SkiaGlyphNameProvider(Typeface typeface) => _typeface = typeface;

    public Glyph GetGlyph(string glyphName) => _typeface.GetGlyphByName(glyphName);

    public string GetGlyphName(Glyph glyph) => glyph.GetCff1GlyphData().Name;
  }
}
