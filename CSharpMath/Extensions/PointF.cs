using System.Drawing;

namespace CSharpMath {
  public static partial class Extensions {
    public static PointF Plus(this PointF point1, PointF point2) =>
      new PointF(point1.X + point2.X, point1.Y + point2.Y);
  }
}