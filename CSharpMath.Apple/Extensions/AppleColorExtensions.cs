using CSharpMath.Structures;

namespace CSharpMath.Apple {
  public static class ColorExtensions {
    public static CoreGraphics.CGColor ToCgColor(this Color color)
      => new CoreGraphics.CGColor(color.Rf, color.Gf, color.Bf, color.Af);
    public static UIKit.UIColor ToUiColor(this Color color)
      => new UIKit.UIColor(color.Rf, color.Gf, color.Bf, color.Af);
  }
}
