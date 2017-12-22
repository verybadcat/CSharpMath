using System;
using UIKit;

namespace CSharpMath.Ios
{
  public class IosMathViewController: UIViewController
  {
    public IosMathViewController()
    {
    }

    private const string QuadraticFormula = @"y = -b \pm \frac{\sqrt{b^2-4ac}}{2a}";
    private const string Radical = @"\sqrt{3}";
    private const string RadicalSum = @"2 + \sqrt{3}";
    private const string Fraction = @"\frac{2}{34}";
    private const string RadicalFraction = @"2+ \frac{\sqrt{3}}{2}";
    private const string IntPlusFraction = @"1+\frac23";

    public override void ViewDidLoad()
    {
      View.BackgroundColor = UIColor.White;
      var latexView = IosMathLabels.LatexView(QuadraticFormula);
      latexView.BackgroundColor = UIColor.LightGray;
      latexView.SizeToFit();
      latexView.Frame = new CoreGraphics.CGRect(0, 20, latexView.Frame.Width, latexView.Frame.Height);
      View.Add(latexView);
    }
  }
}
