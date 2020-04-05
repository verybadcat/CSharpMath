using System;
using CSharpMath.Structures;

namespace CSharpMath.Rendering.FrontEnd {
  using FrontEnd;
  [Obsolete("Not ready", true)]
  public class InvertedCanvas : ICanvas {
    public InvertedCanvas(ICanvas canvas) =>
      _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas), "The supplied canvas cannot be null.");
    private readonly ICanvas _canvas;
    float Invert(float origY) => origY * -1 + _canvas.Height;
    public float Width => _canvas.Width;
    public float Height => _canvas.Height;
    public Color DefaultColor
      { get => _canvas.DefaultColor; set => _canvas.DefaultColor = value; }
    public Color? CurrentColor
      { get => _canvas.CurrentColor; set => _canvas.CurrentColor = value; }
    public PaintStyle CurrentStyle
      { get => _canvas.CurrentStyle; set => _canvas.CurrentStyle = value; }
    public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness) =>
      _canvas.DrawLine(x1, Invert(y1), x2, Invert(y2), lineThickness);
    public void FillRect(float left, float top, float width, float height) =>
      _canvas.FillRect(left, Invert(top), width, height);
    public GlyphPath StartDrawingNewGlyph() => _canvas.StartDrawingNewGlyph();
    public void Restore() => _canvas.Restore();
    public void Save() => _canvas.Save();
    public void Scale(float sx, float sy) => _canvas.Scale(sx, sy);
    public void StrokeRect(float left, float top, float width, float height) =>
      _canvas.StrokeRect(left, Invert(top), width, height);
    public void Translate(float dx, float dy) => _canvas.Translate(dx, dy);
  }
}
