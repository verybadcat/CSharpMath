namespace CSharpMath.Apple {
  using System.Drawing;
  partial class Extensions {
    public static CoreGraphics.CGColor ToCGColor(this Color color) =>
      new CoreGraphics.CGColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    public static UIKit.UIColor ToUIColor(this Color color) =>
      new UIKit.UIColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
  }
}