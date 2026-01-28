using System.Drawing;
using CSharpMath.Rendering.FrontEnd;
using VectSharp;

namespace CSharpMath.VectSharp;

public sealed class VectSharpCanvas(Page canvas) : ICanvas {
  public Page Canvas { get; } = canvas;
  public float Width => (float)Canvas.Width;
  public float Height => (float)Canvas.Height;
  public Color DefaultColor { get; set; }
  public Color? CurrentColor { get; set; }
  public PaintStyle CurrentStyle { get; set; }
  public void StrokeRect(float left, float top, float width, float height) =>
      Canvas.Graphics.StrokeRectangle(left, top, width, height, (CurrentColor ?? DefaultColor).ToNative());
  public void FillRect(float left, float top, float width, float height) =>
      Canvas.Graphics.FillRectangle(left, top, width, height, (CurrentColor ?? DefaultColor).ToNative());
  public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness) {
    if (CurrentStyle == PaintStyle.Fill)
      Canvas.Graphics.StrokePath(new GraphicsPath().MoveTo(x1, y1).LineTo(x2, y2), (CurrentColor ?? DefaultColor).ToNative(), lineThickness);
    else
      this.StrokeLineOutline(x1, y1, x2, y2, lineThickness);
  }
  public void Save() => Canvas.Graphics.Save();
  public void Translate(float dx, float dy) => Canvas.Graphics.Translate(dx, dy);
  public void Scale(float sx, float sy) => Canvas.Graphics.Scale(sx, sy);
  public void Restore() => Canvas.Graphics.Restore();
  public Path StartNewPath() => new VectSharpPath(this);
}