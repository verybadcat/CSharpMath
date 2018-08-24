using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

//For the Android linker
namespace Android.Runtime {
  public sealed class PreserveAttribute : Attribute {
    public bool AllMembers;
    public bool Conditional;
  }
}

//For the iOS linker
namespace Foundation {
  public sealed class PreserveAttribute : Attribute {
    public bool AllMembers;
    public bool Conditional;
  }
}

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile), Android.Runtime.Preserve(AllMembers = true), Foundation.Preserve(AllMembers = true)]
  public partial class ExamplesPage : ContentPage {
    public const string Numbers = @"0123456789";
    public const string Alphabets = @"abcdefghijklmnopqrstuvwxyz";
    public const string Capitals = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string Color = @"\color{#008}a\color{#00F}b\color{#080}c\color{#088}d\color{#08F}e\color{#0F0}f\color{#0F8}g\color{#0FF}h\color{#800}i\color{#808}j\color{#80F}k\color{#880}l\color{#888}m\color{#88F}n\color{#8F0}o\color{#8F8}p\color{#8FF}q\color{#F00}r\color{#F08}s\color{#F0F}t\color{#F80}u\color{#F88}v\color{#F8F}w\color{#FF0}x\color{#FF8}y\color{#FFF}z";
    
    //Refer to CSharpMath\CSharpMath.Utils\CSharpMathExamples\MirrorFromIos.cs
    //START mirror CSharpMath.Ios.Example
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

    public const string Taylor = @"\begin{eqnarray} e^x  &=&  \sum_{x=0}^{\infty}\frac{x^n}{n!} \\ \\ \sin(x) &=& \sum_{x=0}^{\infty}(-1)^n\frac{(2x+1)^n}{(2n)!}  \\ \\ -\ln(1-x)   &=& \sum_{x=0}^{\infty}\frac{x^n}{n}  \ \ \ \ \ (-1 \leq x < 1) \end{eqnarray}";
    public const string TwoSin = @"2 \sin";
    public const string Underline = @"\underline{Underline}";

    //END mirror CSharpMath.Ios.Example

    public const string BMartix = @"\begin{bmatrix} x_{11}&x_{12}&x_{13}&.&.&.&.&x_{1n} \end{bmatrix}";
    public const string SolveEquations = @"\text{Solve } \begin{cases} y=x^2-x+3 \\ y=x^2+\sqrt x-\frac2x \end{cases} \text{ using the graphical method.}";
    public const string Abs = @"|x|=\begin{cases} -x, & \text{ if } x < 0 \\ x, & \text{ if } x \geq 0 \end{cases}";
    public const string SummationWithBigLimits = @"\sum^{4^{5^{6^{7^{8^{9^{10^{11^{12^{13}}}}}}}}}}_{i=3_{2_{1_{0_{-1_{-2_{-3}}}}}}}i";
    //\sum^{a^{s^{c^{e^{n^{d^{i^{n^g}}}}}}}}_{d_{e_{s_{c_{e_{n_{d_{i_{n_g}}}}}}}}}normal
    public const string EvalIntegral = @"\int_1^2 x\; dx=\left.\frac{x^2}{2}\right|_1^2=4-\frac{1}{2}=\frac{7}{2}";
#warning Uncomment when \middle is implemented
    //public const string MiddleDelimiter = @"A = \left\{ \frac{x_i}{i} \middle| i\in \mathcal{I} \right\}";
    public const string VectorProjection = @"Proj_\vec{v}\vec{u}=|\vec u|\cos\theta\times\frac\vec v{|\vec v|}=|\vec u|\frac{\vec u \cdot \vec v}{|\vec u||\vec v|}\times\frac\vec v{|\vec v|}\\\text{Suppose \mathit{u} and \mathit v are unit vectors, }Proj_\vec v\vec u = (\vec u\cdot\vec v)\vec v";
    
    public ExamplesPage() => InitializeComponent();

    public static IEnumerable<FieldInfo> AllConstants { get; } = typeof(ExamplesPage).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).
      Where(fi => fi.IsLiteral && !fi.IsInitOnly);
    static readonly string latex = string.Join(@"\\", AllConstants.Select(info => $@"{info.Name}: {info.GetRawConstantValue()}"));

    protected override void OnAppearing() {
      base.OnAppearing();
      App.AllViews.Add(View);
      View.FontSize = 50;
      View.LaTeX = latex;
      View.InvalidateSurface();
    }
    protected override void OnDisappearing() {
      //App.AllViews.Remove(View);
      base.OnDisappearing();
    }
  }
}
