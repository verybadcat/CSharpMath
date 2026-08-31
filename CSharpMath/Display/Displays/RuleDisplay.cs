using System.Drawing;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  /// <summary>A standalone straight rule (horizontal or vertical) used to draw array
  /// `|` and \hline. A positioned leaf like GlyphDisplay: it carries its own
  /// position/width/ascent/descent so it folds into the enclosing table's bounds.
  /// `start` is a point on the stroke's centre-line; the stroke straddles it by
  /// thickness/2 on the thickness axis.</summary>
  public class RuleDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    private readonly float _length;
    private readonly float _thickness;
    private readonly bool _vertical;

    public RuleDisplay(PointF centreLineStart, float length, float thickness,
      bool vertical, Range range) {
      _length = length;
      _thickness = thickness;
      _vertical = vertical;
      Range = range;
      // Record Position as the box's lower-left origin so the display's bounds
      // exactly cover the drawn stroke.
      if (vertical) {
        Position = new PointF(centreLineStart.X - thickness / 2, centreLineStart.Y);
        Width = thickness;
        Ascent = length;
        Descent = 0;
      } else {
        Position = new PointF(centreLineStart.X, centreLineStart.Y - thickness / 2);
        Width = length;
        Ascent = thickness;
        Descent = 0;
      }
    }

    public Range Range { get; }
    public float Width { get; set; }
    public float Ascent { get; set; }
    public float Descent { get; set; }
    public PointF Position { get; set; }
    public bool HasScript { get; set; }
    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) => TextColor ??= textColor;
    public Color? BackColor { get; set; }

    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      // Re-derive the centre-line from the box origin.
      float beginX = _vertical ? Position.X + _thickness / 2 : Position.X;
      float beginY = _vertical ? Position.Y : Position.Y + _thickness / 2;
      float endX = _vertical ? beginX : beginX + _length;
      float endY = _vertical ? beginY + _length : beginY;
      context.DrawLine(beginX, beginY, endX, endY, _thickness, TextColor);
    }
    public override string ToString() => "rule";
  }
}
