using CSharpMath.Structures;

namespace CSharpMath.Apple {
  public static class ColorExtensions {
    public static CoreGraphics.CGColor ToCgColor(this Color color)
      => new CoreGraphics.CGColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    public static UIKit.UIColor ToUiColor(this Color color)
      => new UIKit.UIColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
  }
}
