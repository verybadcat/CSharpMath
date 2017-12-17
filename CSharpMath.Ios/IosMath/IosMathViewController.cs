using System;
using UIKit;

namespace CSharpMath.Ios
{
  public class IosMathViewController: UIViewController
  {
    public IosMathViewController()
    {
    }

    public override void ViewDidLoad()
    {
      View.BackgroundColor = UIColor.White;
      var latexView = IosMathLabels.LatexView(@"\sqrt{b-4ac}");
      latexView.BackgroundColor = UIColor.LightGray;
      latexView.SizeToFit();
      latexView.Frame = new CoreGraphics.CGRect(0, 20, latexView.Frame.Width, latexView.Frame.Height);
      View.Add(latexView);
    }
  }
}
