using UIKit;

namespace CSharpMath.Ios.Example {
  public class IosMathViewController : UIViewController {
    public override void ViewDidLoad() {
      View.BackgroundColor = UIColor.White;
      var latexView = IosMathLabels.MathView(Rendering.Tests.TestRenderingMathData.IntegralColorBoxCorrect, 50);  // WJWJWJ latex here
      latexView.ContentInsets = new UIEdgeInsets(10, 10, 10, 10);
      var size = latexView.SizeThatFits(new CoreGraphics.CGSize(370, 280));
      latexView.Frame = new CoreGraphics.CGRect(0, 40, size.Width, size.Height);
      View.Add(latexView);
    }
  }
}
