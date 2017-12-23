using System;
namespace CSharpMath.Display.Text
{
  public class KernedGlyph<TGlyph>
  {
    public KernedGlyph(TGlyph glyph, float kern = 0) {
      Glyph = glyph;
      KernAfterGlyph = kern;
    }
    public TGlyph Glyph { get; set; }
    public float KernAfterGlyph { get; set; }
  }
}
