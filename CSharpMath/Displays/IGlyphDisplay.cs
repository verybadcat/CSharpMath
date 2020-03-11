namespace CSharpMath.Displays {
  public interface IGlyphDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph> where TFont : FrontEnd.IFont<TGlyph> {
    float ShiftDown { get; set; }
  }
}