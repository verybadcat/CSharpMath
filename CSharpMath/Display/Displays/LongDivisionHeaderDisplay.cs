using System.Drawing;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;

  internal sealed class LongDivisionHeaderDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    readonly IDisplay<TFont, TGlyph> _dividend;
    readonly IGlyphDisplay<TFont, TGlyph> _delimiter;
    readonly float _lineShiftUp;
    readonly float _lineThickness;

    internal LongDivisionHeaderDisplay(IDisplay<TFont, TGlyph> dividend,
      IGlyphDisplay<TFont, TGlyph> delimiter, float lineShiftUp,
      float lineThickness, Range range) {
      _dividend = dividend;
      _delimiter = delimiter;
      _lineShiftUp = lineShiftUp;
      _lineThickness = lineThickness;
      Range = range;
    }

    public float Ascent => System.Math.Max(_lineShiftUp + _lineThickness / 2,
      System.Math.Max(_dividend.Position.Y - Position.Y + _dividend.Ascent,
        _delimiter.Position.Y - Position.Y + _delimiter.Ascent));
    public float Descent => System.Math.Max(-_lineShiftUp + _lineThickness / 2,
      System.Math.Max(-(_dividend.Position.Y - Position.Y) + _dividend.Descent,
        -(_delimiter.Position.Y - Position.Y) + _delimiter.Descent));
    public float Width => System.Math.Max(_dividend.Position.X - Position.X + _dividend.Width,
      _delimiter.Position.X - Position.X + _delimiter.Width);
    public Range Range { get; }
    PointF _position;
    public PointF Position {
      get => _position;
      set {
        var delta = new PointF(value.X - _position.X, value.Y - _position.Y);
        _position = value;
        _dividend.Position = new PointF(_dividend.Position.X + delta.X, _dividend.Position.Y + delta.Y);
        _delimiter.Position = new PointF(_delimiter.Position.X + delta.X, _delimiter.Position.Y + delta.Y);
      }
    }
    public bool HasScript { get; set; }
    public Color? TextColor { get; set; }
    public Color? BackColor { get; set; }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      _delimiter.Draw(context);
      _dividend.Draw(context);
      context.SaveState();
      context.DrawLine(Position.X, Position.Y + _lineShiftUp,
        Position.X + Width, Position.Y + _lineShiftUp, _lineThickness, TextColor);
      context.RestoreState();
    }
    public void SetTextColorRecursive(Color? textColor) {
      TextColor ??= textColor;
      _delimiter.SetTextColorRecursive(textColor);
      _dividend.SetTextColorRecursive(textColor);
    }
  }
}
