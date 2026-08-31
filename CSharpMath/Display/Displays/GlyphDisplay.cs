using System.Drawing;
using System.Linq;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  public class GlyphDisplay<TFont, TGlyph> : IGlyphDisplay<TFont, TGlyph>, IInkDisplay
    where TFont : IFont<TGlyph> {

    readonly float _ascent;
    readonly float _descent;
    public float Ascent => _ascent - ShiftDown;
    public float Descent => _descent + ShiftDown;
    public float Width { get; }
    public float InkWidth { get; }
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
      InkWidth = GetInkWidth(glyph, font, width);
    }
    static float GetInkWidth(TGlyph glyph, TFont font, float width) {
      if (font is IFontGlyphBounds<TGlyph> bounds)
        return System.Math.Max(width, bounds.GetBoundingRects(new[] { glyph }).FirstOrDefault().Right);
      return width;
    }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      context.SaveState();
      using var glyphs = new RentedArray<TGlyph>(Glyph);
      using var positions = new RentedArray<PointF>(new PointF());
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
