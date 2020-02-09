namespace CSharpMath.Display.Text {
  public interface IGlyphDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph> where TFont : IFont<TGlyph> {
    float ShiftDown { get; set; }
  }
}