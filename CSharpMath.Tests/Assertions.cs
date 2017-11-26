using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CSharpMath.Tests {
  public static class Assertions {
    public static void ApproximatelyEquals(double expected, double actual, double tolerance) {
      var delta = actual - expected;
      Assert.True(Math.Abs(delta) <= tolerance);
    }

    public static void ApproximatelyEquals(RectangleF expected, RectangleF actual, double tolerance) {
      ApproximatelyEquals(expected.X, actual.X, tolerance);
      ApproximatelyEquals(expected.Y, actual.Y, tolerance);
      ApproximatelyEquals(expected.Width, actual.Width, tolerance);
      ApproximatelyEquals(expected.Height, actual.Height, tolerance);
    }

    public static void ApproximatelyEquals(RectangleF actual, double x, double y, double width, double height, double tolerance)
      => ApproximatelyEquals(new RectangleF((float)x, (float)y, (float)width, (float)height), actual, tolerance);
  }
}
