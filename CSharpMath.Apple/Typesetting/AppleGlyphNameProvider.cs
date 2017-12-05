using System;
using CoreText;
using TGlyph = System.UInt16;
namespace CSharpMath.Apple
{
  public class AppleGlyphNameProvider: IGlyphNameProvider<TGlyph>
  {
    private readonly CTFont _ctFont;

    public AppleGlyphNameProvider(CTFont ctFont) {
      _ctFont = ctFont;
    }
    public ushort GetGlyph(string glyphName)
    {
      return _ctFont.GetGlyphWithName(glyphName);
    }

    public string GetGlyphName(ushort glyph)
    {
      var cgFont = _ctFont.ToCGFont();
      return cgFont.GlyphNameForGlyph(glyph);
    }
  }
}
