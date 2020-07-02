using System.Drawing;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  public class GlyphDisplay<TFont, TGlyph> : IGlyphDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {

    readonly float _ascent;
    readonly float _descent;
    public float Ascent => _ascent - ShiftDown;
    public float Descent => _descent + ShiftDown;
    public float Width { get; }
    public Range Range { get; }
    public PointF Position { get; set; }
    public bool HasScript { get; set; }
    public float ShiftDown { get; set; }
    public TGlyph Glyph { get; }
    public TFont Font { get; }
    public GlyphDisplay(TGlyph glyph, Range range, TFont font,
      float ascent, float descent, float width) {
      Glyph = glyph;
      Range = range;
      Font = font;
      _ascent = ascent;
      _descent = descent;
      Width = width;
    }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
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
    public Color? BackColor { get; set; }
    public override string ToString() => Glyph?.ToString() ?? "<null>";
  }
}
