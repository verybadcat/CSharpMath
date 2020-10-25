namespace CSharpMath.Rendering.FrontEnd {
  using System;
  using System.Drawing;

  using Editor;
  using BackEnd;

  public enum CaretShape { IBeam, UpArrow }
  public class MathKeyboard : MathKeyboard<Fonts, Glyph> {
    public MathKeyboard(float fontSize = PainterConstants.DefaultFontSize, double blinkMilliseconds = DefaultBlinkMilliseconds)
      : base(TypesettingContext.Instance,
             new Fonts(Array.Empty<Typography.OpenFont.Typeface>(), fontSize), blinkMilliseconds) { }
    // Rendering: Convert to 
    public override RectangleF Measure =>
      Display != null ? new RectangleF(0, -Display.Ascent, Display.Width, Display.Ascent + Display.Descent) : RectangleF.Empty;
    public void DrawCaret(ICanvas canvas, Color color, CaretShape shape) {
      if (Display == null)
        return;
      var cursorPosition = Display.PointForIndex(TypesettingContext.Instance, InsertionIndex) ?? Display.Position;
      cursorPosition.Y *= -1; //inverted canvas, blah blah
      using var path = canvas.StartNewPath();
      path.Foreground = color;
      path.MoveTo(cursorPosition.X, cursorPosition.Y);
      switch (shape) {
        case CaretShape.IBeam:
          ReadOnlySpan<PointF> s = stackalloc PointF[4] {
            new PointF(Font.PointSize / 2 / 16, 0),
            new PointF(Font.PointSize / 2 / 16, -Font.PointSize * 2 / 3),
            new PointF(-Font.PointSize / 2 / 16, -Font.PointSize * 2 / 3),
            new PointF(-Font.PointSize / 2 / 16, 0),
          };
          foreach (var p in s)
            path.LineTo(p.X + cursorPosition.X, p.Y + cursorPosition.Y);
          break;
        case CaretShape.UpArrow:
          s = stackalloc PointF[4] {
            new PointF(Font.PointSize / 2 / 2, Font.PointSize * 2 / 3 / 4),
            new PointF(Font.PointSize / 2 / 2, Font.PointSize * 2 / 3),
            new PointF(-Font.PointSize / 2 / 2, Font.PointSize * 2 / 3),
            new PointF(-Font.PointSize / 2 / 2, Font.PointSize * 2 / 3 / 4)
          };
          foreach (var p in s)
            path.LineTo(p.X + cursorPosition.X, p.Y + cursorPosition.Y);
          break;
      }
      path.CloseContour();
    }
  }
}