namespace CSharpMath.Display {
  public interface IGlyphDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph> where TFont : FrontEnd.IFont<TGlyph> {
    float ShiftDown { get; set; }
    TFont Font { get; }
  }
}