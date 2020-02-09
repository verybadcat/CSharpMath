using System;
using System.Drawing;

namespace CSharpMath {
  public static class RectangleFExtensions {
    public static RectangleF Plus(this RectangleF rect, PointF vector) =>
      new RectangleF(rect.Location.Plus(vector), rect.Size);
    public static RectangleF Union(this RectangleF rect1, RectangleF rect2) {
      var x = Math.Min(rect1.X, rect2.X);
      var y = Math.Min(rect1.Y, rect2.Y);
      var maxX = Math.Max(rect1.Right, rect2.Right);
      var maxY = Math.Max(rect1.Bottom, rect2.Bottom);
      return new RectangleF(x, y, maxX - x, maxY - y);
    }
    /// <summary>Because we are NOT inverting our y axis,
    /// the properties "Top" and "Bottom" have misleading names.</summary>
    public static float YMin(this RectangleF rect) => rect.Top;
    /// <summary>Because we are NOT inverting our y axis,
    /// the properties "Top" and "Bottom" have misleading names.</summary>
    public static float YMax(this RectangleF rect) => rect.Bottom;
    public static void GetAscentDescentWidth(this RectangleF rect,
      out float ascent, out float descent, out float width) {
      ascent = rect.Bottom;
      width = rect.Width;
      descent = -rect.Y;
    }
  }
}
