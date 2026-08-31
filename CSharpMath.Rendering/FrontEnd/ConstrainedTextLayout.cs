namespace CSharpMath.Rendering.FrontEnd {
  internal static class ConstrainedTextLayout {
    internal static float ContentWidth(double available, double left, double right) =>
      double.IsInfinity(available) || double.IsNaN(available)
      ? (float)available
      : (float)System.Math.Max(0, available - left - right);
  }
}
