using System;
using System.Drawing;

namespace CSharpMath.Rendering.FrontEnd {
  public interface ICanvas {
    float Width { get; }
    float Height { get; }
    Color DefaultColor { get; set; }
    Color? CurrentColor { get; set; }
    PaintStyle CurrentStyle { get; set; }
    /// <summary>The path must be drawn to canvas on disposal and respect Color and Styling properties.</summary>
    Path StartNewPath();
    /// <summary>Implementations can choose whether to respect <see cref="CurrentStyle"/> or not by calling <see cref="CanvasExtensions.StrokeLineOutline(ICanvas, float, float, float, float, float)"/> for stroking.</summary>
    void DrawLine(float x1, float y1, float x2, float y2, float lineThickness);
    /// <summary>A arbitrarily small stroke thickness is assumed, for use in drawing glyph boxes.</summary>
    void StrokeRect(float left, float top, float width, float height);
    /// <summary>For filling glyph highlights.</summary>
    void FillRect(float left, float top, float width, float height);
    /// <summary>Implementations are only required to save Translate and Scale transforms. Saving color and style properties is optional.</summary>
    void Save();
    void Translate(float dx, float dy);
    void Scale(float sx, float sy);
    /// <summary>Implementations are only required to restore Translate and Scale transforms. Restoring color and style properties is optional.</summary>
    void Restore();
  }
  public static class CanvasExtensions {
    public static void StrokeLineOutline
      (this ICanvas c, float x1, float y1, float x2, float y2, float lineThickness) {
      var dx = Math.Abs(x2 - x1);
      var dy = Math.Abs(y2 - y1);
      var length = (float)Math.Sqrt((double)dx * dx + (double)dy * dy);
      var halfThickness = lineThickness / 2;
      var px = dx / length * halfThickness;
      var py = dy / length * halfThickness;
      using var p = c.StartNewPath();
      p.MoveTo(x1 - py, y1 + px);
      p.LineTo(x1 + py, y1 - px);
      p.LineTo(x2 + py, y2 - px);
      p.LineTo(x2 - py, y2 + px);
      p.CloseContour();
    }
  }
}