using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Structures;

namespace CSharpMath.Rendering {
  public interface ICanvas : Typography.OpenFont.IGlyphTranslator {
    float Width { get; }
    float Height { get; }
    Color DefaultColor { get; set; }
    Color? CurrentColor { get; set; }
    PaintStyle CurrentStyle { get; set; }

    void FillColor();
    void StrokeRect(float left, float top, float width, float height);
    void AddLine(float x1, float y1, float x2, float y2, float lineThickness);
    void Save();
    void Translate(float dx, float dy);
    void Scale(float sx, float sy);
    void Restore();
    void FillText(string text, float x, float y, float pointSize);
  }
  public static class CanvasExtensions {
    public static void StrokeLineOutline(this ICanvas c, float x1, float y1, float x2, float y2, float lineThickness) {
      var dx = Math.Abs(x2 - x1);
      var dy = Math.Abs(y2 - y1);
      var length = (float)Math.Sqrt((double)dx * dx + (double)dy * dy);
      var halfThickness = lineThickness / 2;
      var px = dx / length * halfThickness;
      var py = dy / length * halfThickness;
      c.BeginRead(0);
      c.MoveTo(x1 - py, y1 + px);
      c.LineTo(x1 + py, y1 - px);
      c.LineTo(x2 + py, y2 - px);
      c.LineTo(x2 - py, y2 + px);
      c.CloseContour();
      c.EndRead();
    }
  }
}