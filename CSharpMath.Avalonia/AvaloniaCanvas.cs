using System.Collections.Generic;

using Avalonia;
using Avalonia.Media;

using CSharpMath.Rendering.FrontEnd;
using CSharpMathColor = System.Drawing.Color;

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
    internal IBrush CurrentBrush { get; private set; } = Brushes.Transparent;
    public CSharpMathColor DefaultColor {
      get => _defaultColor;
      set {
        _defaultColor = value;
        if (_currentColor == null)
          CurrentBrush = value.ToSolidColorBrush();
      }
    }
    CSharpMathColor _defaultColor;
    CSharpMathColor? _currentColor;
    public CSharpMathColor? CurrentColor {
      get => _currentColor;
      set {
        _currentColor = value;
        CurrentBrush = (value ?? _defaultColor).ToSolidColorBrush();
      }
    }
    public PaintStyle CurrentStyle { get; set; }
    internal DrawingContext DrawingContext { get; }
    public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness) {
      if (CurrentStyle == PaintStyle.Fill)
        DrawingContext.DrawLine(new Pen(CurrentBrush, lineThickness), new Point(x1, y1), new Point(x2, y2));
      else this.StrokeLineOutline(x1, y1, x2, y2, lineThickness);
    }
    public void FillRect(float left, float top, float width, float height) =>
        DrawingContext.FillRectangle(CurrentBrush, new Rect(left, top, width, height));
    public Path StartNewPath() => new AvaloniaPath(this);
    public void Restore() {
      var stateStack = _states.Pop();
      while (stateStack.Count > 0) {
        stateStack.Pop().Dispose();
      }
    }
    public void Save() => _states.Push(new Stack<DrawingContext.PushedState>());
    public void Scale(float sx, float sy) =>
        PushState(DrawingContext.PushTransform(Matrix.CreateScale(sx, sy)));
    public void StrokeRect(float left, float top, float width, float height) =>
        DrawingContext.DrawRectangle(new Pen(CurrentBrush), new Rect(left, top, width, height));
    public void Translate(float dx, float dy) =>
        PushState(DrawingContext.PushTransform(Matrix.CreateTranslation(dx, dy)));
    private void PushState(DrawingContext.PushedState state) => _states.Peek().Push(state);
  }
}