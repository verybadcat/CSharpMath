using CoreGraphics;
using TGlyph = System.UInt16;
namespace CSharpMath.Apple {
  public class AppleGlyphNameProvider : FrontEnd.IGlyphNameProvider<TGlyph> {
    private readonly CGFont _cgFont;
    public AppleGlyphNameProvider(CGFont cgFont) => _cgFont = cgFont;
    public TGlyph GetGlyph(string glyphName) => _cgFont.GetGlyphWithGlyphName(glyphName);
    public string GetGlyphName(TGlyph glyph) => _cgFont.GlyphNameForGlyph(glyph);
  }
}