using static System.Array;
using TGlyph = System.UInt16;
using Typography.OpenFont;
namespace CSharpMath.SkiaSharp
{
  public class SkiaGlyphNameProvider: IGlyphNameProvider<TGlyph>
  {
    private readonly Typeface _typeface;

    public SkiaGlyphNameProvider(Typeface typeface) => _typeface = typeface;

    public TGlyph GetGlyph(string glyphName) => System.Array.Find(_typeface.Glyphs, g => g);

    public string GetGlyphName(TGlyph glyph)
    {
      var cgFont = _ctFont.ToCGFont();
      return cgFont.GlyphNameForGlyph(glyph);
    }
  }
}
