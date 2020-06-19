using UIKit;

namespace CSharpMath.Ios.Example {
  public class IosMathViewController : UIViewController {
    public override void ViewDidLoad() {
      var scrollView = new UIScrollView { BackgroundColor = UIColor.White, ScrollEnabled = true };
      System.nfloat y = 0, w = 0;
      foreach (var latex in Rendering.Tests.TestRenderingMathData.AllConstants.Values) {
        var latexView = IosMathLabels.MathView(latex, 50);  // WJWJWJ latex here
        var size = latexView.SizeThatFits(new CoreGraphics.CGSize(370, 280));
        latexView.Frame = new CoreGraphics.CGRect(0, y, size.Width, size.Height);
        scrollView.Add(latexView);
        y += size.Height;
        w = size.Width > w ? size.Width : w;
        y += 10;
      }
      scrollView.ContentSize = new CoreGraphics.CGSize(w, y);
      View = scrollView;
    }
  }
}
