using System;
using UIKit;

namespace CSharpMath.Ios
{
  public class IosMathViewController: UIViewController
  {
    public IosMathViewController()
    {
    }

    private const string QuadraticFormula = @"x = -b \pm \frac{\sqrt{b^2-4ac}}{2a}";
    private const string NestedRadical = @"\sqrt{\sqrt{x}}";
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
      var size = latexView.SizeThatFits(new CoreGraphics.CGSize(370, 180));
      latexView.Frame = new CoreGraphics.CGRect(0, 20, 320, 180);
      View.Add(latexView);
    }
  }
}
