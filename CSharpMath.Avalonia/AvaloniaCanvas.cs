using System.Collections.Generic;

using Avalonia;
using Avalonia.Media;

using CSharpMath.Rendering.FrontEnd;
using CSharpMathColor = CSharpMath.Structures.Color;

namespace CSharpMath.Avalonia {
  public sealed class AvaloniaCanvas : ICanvas {
    private readonly Stack<Stack<DrawingContext.PushedState>> _states =
      new Stack<Stack<DrawingContext.PushedState>>();
    public AvaloniaCanvas(DrawingContext drawingContext, Size size) {
      DrawingContext = drawingContext;

      Width = (float)size.Width;
      Height = (float)size.Height;
    }
    public float Width { get; }
    public float Height { get; }
    public CSharpMathColor DefaultColor { get; set; }
    public CSharpMathColor? CurrentColor { get; set; }
    public PaintStyle CurrentStyle { get; set; }
    internal DrawingContext DrawingContext { get; }
    internal IBrush CurrentBrush => new SolidColorBrush((CurrentColor ?? DefaultColor).ToAvaloniaColor());
    public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness) {
      if (CurrentStyle == PaintStyle.Fill)
        DrawingContext.DrawLine(new Pen(CurrentBrush, lineThickness), new Point(x1, y1), new Point(x2, y2));
      else this.StrokeLineOutline(x1, y1, x2, y2, lineThickness);
    }
    public void FillRect(float left, float top, float width, float height) =>
        DrawingContext.FillRectangle(CurrentBrush, new Rect(left, top, width, height));
    public void FillText(string text, float x, float y, float pointSize) {
      var typeface = new Typeface(Typeface.Default.FontFamily, pointSize);
      DrawingContext.DrawText(
          CurrentBrush,
          new Point(x, y),
          new FormattedText() { Text = text, Typeface = typeface });
    }
    public GlyphPath StartDrawingNewGlyph() => new AvaloniaPath(this);
    public void Restore() {
      var stateStack = _states.Pop();
      while (stateStack.Count > 0) {
        stateStack.Pop().Dispose();
      }
    }
    public void Save() => _states.Push(new Stack<DrawingContext.PushedState>());
    public void Scale(float sx, float sy) =>
        PushState(DrawingContext.PushPreTransform(new Matrix(sx, 0, 0, sy, 0, 0)));
    public void StrokeRect(float left, float top, float width, float height) =>
        DrawingContext.DrawRectangle(new Pen(CurrentBrush), new Rect(left, top, width, height));
    public void Translate(float dx, float dy) =>
        PushState(DrawingContext.PushPreTransform(new Matrix(1, 0, 0, 1, dx, dy)));
    private void PushState(DrawingContext.PushedState state) => _states.Peek().Push(state);
  }
}
