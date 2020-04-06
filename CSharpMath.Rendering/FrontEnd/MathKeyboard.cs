namespace CSharpMath.Rendering.FrontEnd {
  using System;
  using System.Drawing;

  using Editor;
  using BackEnd;

  public enum CaretShape { IBeam, UpArrow }
  public class MathKeyboard : MathKeyboard<Fonts, Glyph> {
    public MathKeyboard() : this(PainterConstants.DefaultFontSize * 3 / 2) { }
    public MathKeyboard(float fontSize = PainterConstants.DefaultFontSize * 3 / 2)
      : base(TypesettingContext.Instance,
             new Fonts(Array.Empty<Typography.OpenFont.Typeface>(), fontSize)) { }
    
    public void DrawCaret(ICanvas canvas, Structures.Color color, CaretShape shape) {
      if (CaretState != MathKeyboardCaretState.Shown || Display is null)
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