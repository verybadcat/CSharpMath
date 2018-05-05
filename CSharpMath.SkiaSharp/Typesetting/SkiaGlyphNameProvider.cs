using Typography.OpenFont;
namespace CSharpMath.SkiaSharp
{
  public class SkiaGlyphNameProvider: IGlyphNameProvider<Glyph>
  {
    private readonly Typeface _typeface;

    public SkiaGlyphNameProvider(Typeface typeface) => _typeface = typeface;
#warning Use GetGlyphByIndex once PR is merged
    public Glyph GetGlyph(string glyphName) => _typeface.CffTable.Cff1FontSet._fonts[0].GetGlyphByName(glyphName);

    public string GetGlyphName(Glyph glyph) => glyph.GetCff1GlyphData().Name;
  }
}
