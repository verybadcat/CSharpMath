using System.Collections.Generic;
using CSharpMath.Rendering.FrontEnd;
using Microsoft.Maui.Graphics;
using CSharpMathCanvas = CSharpMath.Rendering.FrontEnd.ICanvas;
using CSharpMathColor = System.Drawing.Color;
using MauiICanvas = Microsoft.Maui.Graphics.ICanvas;

namespace CSharpMath.Maui {
  public sealed class MauiCanvas : CSharpMathCanvas {
    public MauiCanvas((MauiICanvas, SizeF) canvas) {
      (Canvas, size) = canvas;
    }
    public MauiICanvas Canvas { get; }
    private SizeF size;
    public float Width => size.Width;
    public float Height => size.Height;
    public CSharpMathColor DefaultColor { get; set; }
    public CSharpMathColor? CurrentColor { get; set; }
    public PaintStyle CurrentStyle { get; set; }

    // Canvas methods
    public void StrokeRect(float left, float top, float width, float height) {
      Canvas.SaveState();
      Canvas.StrokeSize = 2;
      Canvas.StrokeColor = (CurrentColor ?? DefaultColor).ToMauiColor();
      Canvas.DrawRectangle(left, top, width, height);
      Canvas.RestoreState();
    }
    public void FillRect(float left, float top, float width, float height) {
      Canvas.SaveState();
      Canvas.StrokeSize = 0;
      Canvas.FillColor = (CurrentColor ?? DefaultColor).ToMauiColor();
      Canvas.FillRectangle(left, top, width, height);
      Canvas.RestoreState();
    }
    public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness) {
      if (CurrentStyle == PaintStyle.Fill) {
        Canvas.SaveState();
        Canvas.StrokeSize = lineThickness;
        Canvas.StrokeColor = (CurrentColor ?? DefaultColor).ToMauiColor();
        Canvas.DrawLine(x1, y1, x2, y2);
        Canvas.RestoreState();
      } else this.StrokeLineOutline(x1, y1, x2, y2, lineThickness);
    }
    public void Save() => Canvas.SaveState();
    public void Translate(float dx, float dy) => Canvas.Translate(dx, dy);
    public void Scale(float sx, float sy) => Canvas.Scale(sx, sy);
    public void Restore() => Canvas.RestoreState();
    public Path StartNewPath() => new MauiPath(this);
  }
}