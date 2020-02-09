using System.Drawing;
using CSharpMath.Atoms;
using CSharpMath.FrontEnd;
using Color = CSharpMath.Structures.Color;

namespace CSharpMath.Display.Text {
  public class GlyphDisplay<TFont, TGlyph> : IGlyphDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    public RectangleF DisplayBounds => this.DisplayBounds();
    public float Ascent { get; set; }
    public float Descent { get; set; }
    public float Width { get; set; }
    public Range Range { get; set; }
    public PointF Position { get; set; }
    public bool HasScript { get; set; }
    public float ShiftDown { get; set; }
    public TGlyph Glyph { get; set; }
    public TFont Font { get; set; }
    public GlyphDisplay(TGlyph glyph, Range range, TFont font) {
      Glyph = glyph;
      Range = range;
      Font = font;
    }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      context.SaveState();
      using var glyphs = new Structures.RentedArray<TGlyph>(Glyph);
      using var positions = new Structures.RentedArray<PointF>(new PointF());
      context.Translate(new PointF(Position.X, Position.Y - ShiftDown));
      context.SetTextPosition(new PointF());
      context.DrawGlyphsAtPoints(glyphs.Result, Font, positions.Result, TextColor);
      context.RestoreState();
    }
    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) => TextColor ??= textColor;
    public override string ToString() => Glyph?.ToString() ?? "<null>";
  }
}
