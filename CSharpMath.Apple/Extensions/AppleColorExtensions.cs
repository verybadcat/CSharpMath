namespace CSharpMath.Apple {
  using Structures;
  public static class ColorExtensions {
    public static CoreGraphics.CGColor ToCGColor(this Color color) =>
      new CoreGraphics.CGColor(color.Rf, color.Gf, color.Bf, color.Af);
    public static UIKit.UIColor ToUIColor(this Color color) =>
      new UIKit.UIColor(color.Rf, color.Gf, color.Bf, color.Af);
  }
}