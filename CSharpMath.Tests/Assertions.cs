using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CSharpMath.Tests {
  public static class Assertions {
    public static void ApproximatelyEqual(double expected, double actual, double tolerance) {
      var delta = actual - expected;
      Assert.True(Math.Abs(delta) <= tolerance);
    }

    public static void ApproximatelyEqual(PointF expected, PointF actual, double tolerance) {
      ApproximatelyEqual(expected.X, actual.X, tolerance);
      ApproximatelyEqual(expected.Y, actual.Y, tolerance);
    }

    public static void ApproximatelyEqual(SizeF expected, SizeF actual, double tolerance) {
      ApproximatelyEqual(expected.Width, actual.Width, tolerance);
      ApproximatelyEqual(expected.Height, actual.Height, tolerance);
    }

    public static void ApproximatelyEqual(RectangleF expected, RectangleF actual, double tolerance) {
      ApproximatelyEqual(expected.Location, actual.Location, tolerance);
      ApproximatelyEqual(expected.Size, actual.Size, tolerance);
    }

    public static void ApproximatelyEquals(RectangleF actual, double x, double y, double width, double height, double tolerance)
      => ApproximatelyEqual(new RectangleF((float)x, (float)y, (float)width, (float)height), actual, tolerance);


  }
}
