using System;
namespace CSharpMath.Display.Text
{
  public class GlyphWithInfo<TGlyph>
  {
    public GlyphWithInfo(TGlyph glyph, float kern = 0) {
      Glyph = glyph;
      KernAfterGlyph = kern;
    }
    public TGlyph Glyph { get; }
    public float KernAfterGlyph { get; set; }
    public Structures.Color Foreground { get; set; }
  }
}
