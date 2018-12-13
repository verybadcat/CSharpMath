namespace CSharpMath.Rendering {
  using System;
  using System.Drawing;

  using Editor;

  public class MathKeyboard : MathKeyboard<Fonts, Glyph> {
    protected MathKeyboard(float fontSize = PainterConstants.DefaultFontSize * 3 / 2)
      : base(TypesettingContext.Instance) { }
    
    public void DrawCaret(ICanvas c) {
      if (!(Caret is CaretHandle caret)) return;
      var path = c.GetPath();
      if (!(Display.PointForIndex(TypesettingContext.Instance, InsertionIndex) is PointF cursorPosition))
        return;
      cursorPosition.Y *= -1; //inverted canvas, blah blah
      var point = caret.InitialPoint.Plus(cursorPosition);
      path.BeginRead(1);
      path.Foreground = caret.ActualColor;
      path.MoveTo(point.X, point.Y);
      var s = (ReadOnlySpan<PointF>)stackalloc PointF[4] {
        caret.NextPoint1, caret.NextPoint2,
        caret.NextPoint3, caret.FinalPoint
      };
      foreach (var p in s)
        path.LineTo(p.X + cursorPosition.X, p.Y + cursorPosition.Y);
      path.CloseContour();
      path.EndRead();
    }
  }
}