namespace CSharpMath.Rendering {
  using System;
  using System.Drawing;

  using Editor;

  public enum CaretShape {
    UpArrow, IBeam
  }
  public class MathKeyboard : MathKeyboard<Fonts, Glyph> {
    public MathKeyboard() : this(PainterConstants.DefaultFontSize * 3 / 2) { }
    public MathKeyboard(float fontSize = PainterConstants.DefaultFontSize * 3 / 2)
      : base(TypesettingContext.Instance) {
      Font = new Fonts(Array.Empty<Typography.OpenFont.Typeface>(), fontSize);
    }
    
    public void DrawCaret(ICanvas c, CaretShape shape = CaretShape.UpArrow) {
      if (!(Caret is CaretHandle caret)) return;
      var path = c.GetPath();
      if (!(Display.PointForIndex(TypesettingContext.Instance, InsertionIndex) is PointF cursorPosition))
        return;
      cursorPosition.Y *= -1; //inverted canvas, blah blah
      var point = caret.InitialPoint.Plus(cursorPosition);
      path.BeginRead(1);
      path.Foreground = caret.ActualColor;
      path.MoveTo(point.X, point.Y);
      switch (shape) {
        default:
        case CaretShape.UpArrow:
          ReadOnlySpan<PointF> s = stackalloc PointF[4] {
            caret.NextPoint1, caret.NextPoint2,
            caret.NextPoint3, caret.FinalPoint
          };
          foreach (var p in s)
            path.LineTo(p.X + cursorPosition.X, p.Y + cursorPosition.Y);
          break;
        case CaretShape.IBeam:
          s = stackalloc PointF[4] {
            new PointF(caret.Bounds.Width / 16, 0),
            new PointF(caret.Bounds.Width / 16, -caret.Bounds.Height),
            new PointF(-caret.Bounds.Width / 16, -caret.Bounds.Height),
            new PointF(-caret.Bounds.Width / 16, 0),
          };
          foreach (var p in s)
            path.LineTo(p.X + cursorPosition.X, p.Y + cursorPosition.Y);
          break;
      }
      path.CloseContour();
      path.EndRead();
    }
  }
}