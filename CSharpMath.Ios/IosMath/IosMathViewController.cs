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
    private const string Matrix = @"\begin{pmatrix}
                           a & b\\ c & d
                            \end{pmatrix}
                            \begin{pmatrix}
                            \alpha & \beta \gamma & \delta
                            \end{pmatrix} = 
                            \begin{pmatrix}
                            a\alpha + b\gamma & a\beta + b \delta
                            c\alpha + d\gamma & c\beta + d \delta 
                            \end{pmatrix}";
    private const string ShortMatrix = @"\begin{pmatrix} a & b\\ c & d \end{pmatrix}";
    private const string VeryShortMatrix = @"\begin{pmatrix}2\end{pmatrix}";
    private const string EmptyMatrix = @"\begin{pmatrix}\end{pmatrix}";
    private const string LeftRight = @"\left(\frac23\right)";
    private const string LeftRightMinus = @"\left(\frac23\right)-";
    private const string LeftSide = @"\frac{1}{\left(\sqrt{\phi \sqrt{5}}-\phi\right) e^{\frac25 \pi}}";
    private const string RightSide = @"1+\frac{e^{-2\pi}} {1 +\frac{e^{-4\pi}} {1+\frac{e^{-6\pi}} {1+\frac{e^{-8\pi}} {1+\cdots} } } }";
    private const string DeeplyNestedFraction = @"\frac{1}{\left(\sqrt{\phi \sqrt{5}}-\phi\right) e^{\frac25 \pi}} = 1+\frac{e^{-2\pi}} {1 +\frac{e^{-4\pi}} {1+\frac{e^{-6\pi}} {1+\frac{e^{-8\pi}} {1+\cdots} } } }";
    private const string DeeplyNestedFraction2 = @"\frac{1}{\left(\sqrt{\phi \sqrt{5}}-\phi\right) e^{\frac25 \pi}} = 1+\frac{e^{-2\pi}} {1 +\frac{e^{-4\pi}} {1+\frac{e^{-6\pi}} {1+\frac{e^{-8\pi}} {1+\cdots} } } }";
    private const string NestedFraction = @"\frac{e^{-6\pi}} {1+\frac{e^{-8\pi}} {1+\cdots} }";
    private const string Exponential = @"e^2";
    private const string ExponentWithFraction = @"e^{4\frac25}";
    private const string ExponentWithProduct = @"e^{2x}";
    private const string ExponentWithPi = @"e^{2\pi}";
    private const string Pi = @"\pi";
    private const string Phi = @"\phi";
    private const string FractionWithRoot = @"\frac{1}{\sqrt{2}}";

    public override void ViewDidLoad()
    {
      View.BackgroundColor = UIColor.White;
      /// WJWJWJ Set latex here.
      var latexView = IosMathLabels.LatexView(ShortMatrix, 15);
      latexView.BackgroundColor = UIColor.LightGray;
      var size = latexView.SizeThatFits(new CoreGraphics.CGSize(370, 180));
      latexView.Frame = new CoreGraphics.CGRect(0, 20, 320, 180);
      View.Add(latexView);
    }
  }
}
