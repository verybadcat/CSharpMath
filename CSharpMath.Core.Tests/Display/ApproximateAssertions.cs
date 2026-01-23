using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using Xunit;

namespace CSharpMath.Core.Tests {
  [DebuggerStepThrough] // Debugger should stop at the line that uses these functions
  public static class Approximately {
    const double DefaultTolerance = 0.001;
    public static void Equal
      (double expected, double actual, double tolerance = DefaultTolerance) =>
      Assert.InRange(actual, expected - tolerance, expected + tolerance);
    public static void Equal
      (SizeF expected, SizeF actual, double tolerance = DefaultTolerance) {
      Equal(expected.Width, actual.Width, tolerance);
      Equal(expected.Height, actual.Height, tolerance);
    }
    public static void At
      (double x, double y, PointF actual, double tolerance = DefaultTolerance) {
      try {
        Equal(x, actual.X, tolerance);
        Equal(y, actual.Y, tolerance);
      } catch (Xunit.Sdk.InRangeException) {
        Assert.Fail($"Expected point at ({x}, {y}) ± {tolerance}, but was ({actual.X}, {actual.Y})");
      }
    }
    public static void Equal
      (PointF? expected, PointF? actual, double tolerance = DefaultTolerance) {
      if (expected is PointF e && actual is PointF a)
        At(e.X, e.Y, a, tolerance);
      else
        Assert.Equal(expected, actual);
    }
    public static void Congruent(double x, double y, double width, double height,
      RectangleF actual, double tolerance = DefaultTolerance) {
      try {
        Equal(x, actual.X, tolerance);
        Equal(y, actual.Y, tolerance);
        Equal(width, actual.Width, tolerance);
        Equal(height, actual.Height, tolerance);
      } catch (Xunit.Sdk.InRangeException) {
        Assert.Fail($"Expected rectangle at ({x}, {y}) with size ({width}, {height}) ± {tolerance}, but was at ({actual.X}, {actual.Y}) with size ({actual.Width}, {actual.Height})");
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Equal
      (RectangleF expected, RectangleF actual, double tolerance = DefaultTolerance) =>
      Congruent(expected.X, expected.Y, expected.Width, expected.Height, actual, tolerance);
  }
}