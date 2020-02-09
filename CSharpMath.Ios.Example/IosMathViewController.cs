using UIKit;

namespace CSharpMath.Ios.Example {
  public class IosMathViewController : UIViewController {
    public IosMathViewController() { }
    public const string Accent = @"\acute{x}";
    public const string Choose = @"{6 \choose x}";
    public const string Commands = @"5\times(-2 \div 1) = -10";

    public const string Exponential = @"e^2";
    public const string ExponentWithFraction = @"e^{4\frac25}";
    public const string ExponentWithProduct = @"e^{2x}";
    public const string ExponentWithPi = @"e^{2\pi}";

    public const string Fraction = @"\frac{2}{34}";
    public const string FractionNested = @"\frac{e^{-6\pi}} {1+\frac{e^{-8\pi}} {1+\cdots} }";
    public const string FractionNestedDeep = @"\frac{1}{\left(\sqrt{\phi \sqrt{5}}-\phi\right) e^{\frac25 \pi}} = 1+\frac{e^{-2\pi}} {1 +\frac{e^{-4\pi}} {1+\frac{e^{-6\pi}} {1+\frac{e^{-8\pi}} {1+\cdots} } } }";
    public const string FractionWithRoot = @"\frac{1}{\sqrt{2}}";
    public const string IntPlusFraction = @"1+\frac23";

    public const string Integral = @"\int_{0}^{\infty}e^x \,dx=\oint_0^{\Delta}5\Gamma";
    public const string LeftRight = @"\left(\frac23\right)";
    public const string LeftRightMinus = @"\left(\frac23\right)-";
    public const string LeftSide = @"\frac{1}{\left(\sqrt{\phi \sqrt{5}}-\phi\right) e^{\frac25 \pi}}";

    public const string Matrix = @"\begin{pmatrix}
                           a & b\\ c & d
                            \end{pmatrix}
                            \begin{pmatrix}
                            \alpha & \beta \\ \gamma & \delta
                            \end{pmatrix} = 
                            \begin{pmatrix}
                            a\alpha + b\gamma & a\beta + b \delta \\
                            c\alpha + d\gamma & c\beta + d \delta 
                            \end{pmatrix}";
    public const string Matrix3 = @"\begin{bmatrix}a&b\\c&d\\e&f\end{bmatrix}";
    public const string Matrix5 = @"\begin{bmatrix}a&b\\c&d\\e&f\\g&h\\i&j\end{bmatrix}";
    public const string Matrix6 = @"\begin{bmatrix}a&b\\c&d\\e&f\\g&h\\i&j\\k&m\end{bmatrix}";
    public const string MatrixEmpty = @"\begin{pmatrix}\end{pmatrix}";
    public const string MatrixShort = @"\begin{pmatrix} a & b\\ c & d \end{pmatrix}";
    public const string MatrixVeryShort = @"\begin{pmatrix}2\end{pmatrix}";

    public const string Overline = @"\overline{Overline}";
    public const string Pi = @"\pi";
    public const string Phi = @"\phi";

    public const string QuadraticFormula = @"x = -b \pm \frac{\sqrt{b^2-4ac}}{2a}";

    public const string Radical = @"\sqrt{3}";
    public const string RadicalNested = @"\sqrt{\sqrt{x}}";
    public const string RadicalNestedDeep = @"\sqrt{\sqrt{\sqrt{\sqrt{\sqrt{\sqrt{\sqrt{\sqrt{x}}}}}}}}";
    public const string RadicalSum = @"2 + \sqrt{3}";
    public const string RadicalFraction = @"2+ \frac{\sqrt{3}}{2}";
    public const string RadicalPower = @"\sqrt{2}^\sqrt{3}";
    public const string RightSide = @"1+\frac{e^{-2\pi}} {1 +\frac{e^{-4\pi}} {1+\frac{e^{-6\pi}} {1+\frac{e^{-8\pi}} {1+\cdots} } } }";

    public const string SomeLimit = @"\lim_{x\to\infty}\frac{e^2}{1-x}=\limsup_{\sigma}5";
    public const string SimpleLimit = @"\lim_{x\to\infty}3=3";
    public const string ShortIntegral = @"\int_0^1";
    public const string SummationWithCup = @"\sum_{n=1}^{\infty}\frac{1+n}{1-n}=\bigcup_{1}C\cup B";
    public const string SummationDouble = @"\sum \sum";
    public const string SummationBigCup = @"234 \bigcup_1";
    public const string SummationWithLimits = @"\sum_{n=1}^{\infty}";

    public const string Taylor = @"\begin{eqnarray} e^x  &=&  \sum_{n=0}^{\infty}\frac{x^n}{n!} \\ \\ \sin(x) &=& \sum_{n=0}^{\infty}(-1)^n\frac{x^{2n+1}}{(2n+1)!}  \\ \\ -\ln(1-x)   &=& \sum_{n=1}^{\infty}\frac{x^n}{n}  \ \ \ \ \ (-1 \leq x < 1) \end{eqnarray}";
    public const string TwoSin = @"2 \sin";
    public const string Underline = @"\underline{Underline}";

    public override void ViewDidLoad() {
      View.BackgroundColor = UIColor.White;
      var latexView = IosMathLabels.MathView(RadicalPower, 50);  // WJWJWJ latex here
      latexView.ContentInsets = new UIEdgeInsets(10, 10, 10, 10);
      var size = latexView.SizeThatFits(new CoreGraphics.CGSize(370, 280));
      latexView.Frame = new CoreGraphics.CGRect(0, 40, size.Width, size.Height);
      View.Add(latexView);
    }
  }
}