using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CSharpMath.Tests {
  [DebuggerStepThrough] // Debugger should stop at the line that uses these functions
  public static class Approximately {
    const double DefaultTolerance = 0.001;

    public static void Equal(double expected, double actual, double tolerance = DefaultTolerance) =>
      Assert.InRange(actual, expected - tolerance, expected + tolerance);

    public static void Equal(PointF expected, PointF actual, double tolerance = DefaultTolerance) {
      Equal(expected.X, actual.X, tolerance);
      Equal(expected.Y, actual.Y, tolerance);
    }

    public static void Equal(SizeF expected, SizeF actual, double tolerance = DefaultTolerance) {
      Equal(expected.Width, actual.Width, tolerance);
      Equal(expected.Height, actual.Height, tolerance);
    }

    public static void Equal(RectangleF expected, RectangleF actual, double tolerance = DefaultTolerance) {
      Equal(expected.Location, actual.Location, tolerance);
      Equal(expected.Size, actual.Size, tolerance);
    }

    public static void At(double x, double y, PointF actual, double tolerance = DefaultTolerance) => 
      Equal(new PointF((float)x, (float)y), actual, tolerance);

    public static void Congruent(double x, double y, double width, double height, RectangleF actual, double tolerance = DefaultTolerance) => 
      Equal(new RectangleF((float)x, (float)y, (float)width, (float)height), actual, tolerance);
  }
}
