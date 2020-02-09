namespace CSharpMath.Displays {
  public class GlyphInfo<TGlyph> {
    public GlyphInfo(TGlyph glyph, float kern = 0) {
      Glyph = glyph;
      KernAfterGlyph = kern;
    }
    public TGlyph Glyph { get; }
    public float KernAfterGlyph { get; set; }
    public Structures.Color? Foreground { get; set; }
    public void Deconstruct(out TGlyph glyph, out float kernAfter, out Structures.Color? foreground) =>
      (glyph, kernAfter, foreground) = (Glyph, KernAfterGlyph, Foreground);
  }
}