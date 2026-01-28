namespace CSharpMath.Rendering.Tests {
  public sealed class TestRenderingMathData : TestRenderingSharedData<TestRenderingMathData> {

    public const string Abs = @"|x|=\begin{cases} -x, & \text{ if } x < 0 \\ x, & \text{ if } x \geq 0 \end{cases}";
    public const string AccentOver = @"\acute{x}";
    //TODO: Fix following line's output
    public const string AccentOverF = @"\hat{f}";
    public const string AccentOverMultiple = @"\widehat{ABcd}";
    public const string AccentUnder = @"\threeunderdot{x}";
    //TODO: Fix following line's output
    public const string AccentUnderThin = @"\threeunderdot{i}";
    public const string ArcsinSin = @"\arcsin(\sin x)=x\quad\mathrm{for}\quad|x|\le\frac\pi2";

    public const string BMartix = @"\begin{bmatrix} x_{11}&x_{12}&x_{13}&.&.&.&.&x_{1n} \end{bmatrix}";
    public const string BraSum = @"\frac{1}{\sqrt{2^n}} \sum_{i=0}^{2^n-1} \Bra{i}";

    public const string Cases = @" w \equiv \begin{cases} 0 & \text{for}\ c = d = 0\\ \sqrt{|c|}\,\sqrt{\frac{1+\sqrt{1+(d/c)^2}}{2}} &\text{for}\ |c| \geq |d|\\ \sqrt{|d|}\,\sqrt{\frac{|c/d| + \sqrt{1+(c/d)^2}}{2}} &\text{for}\ |c|<|d| \end{cases}";
    public const string Choose = @"{6 \choose x}";
    public const string Commands = @"5\times(-2 \div 1) = -10";

    public const string EvalIntegral = @"\int_1^2 x\; dx=\left.\frac{x^2}{2}\right|_1^2=2-\frac{1}{2}=\frac{3}{2}";
    public const string Exponential = @"e^2";
    public const string ExponentWithFraction = @"e^{4\frac25}";
    public const string ExponentWithProduct = @"e^{2x}";
    public const string ExponentWithPi = @"e^{2\pi}";

    public const string FontStyles = @"\mathnormal F\mathrm F\mathbf F\mathcal F\mathtt F\mathit F\mathsf F\mathfrak F\mathbb F\mathbfit F";
    public const string Fraction = @"\frac{2}{34}";
    public const string FractionNested = @"\frac{e^{-6\pi}} {1+\frac{e^{-8\pi}} {1+\cdots} }";
    public const string FractionNestedDeep = @"\frac{1}{\left(\sqrt{\phi \sqrt{5}}-\phi\right) e^{\frac25 \pi}} = 1+\frac{e^{-2\pi}} {1 +\frac{e^{-4\pi}} {1+\frac{e^{-6\pi}} {1+\frac{e^{-8\pi}} {1+\cdots} } } }";
    public const string FractionWithRoot = @"\frac{1}{\sqrt{2}}";
    public const string FunctionDomainCodomain = @"f\colon\mathbb N\rightarrow\mathbb N";

    public const string IntPlusFraction = @"1+\frac23";
    public const string Integral = @"\int_{0}^{\infty}e^x \,dx=\oint_0^{\Delta}5\Gamma";
    public const string IntegralColorBoxCorrect = @"\colorbox{red}{\int^\colorbox{yellow}\infty_\colorbox{purple}{-\infty}}\colorbox{green}x\ \colorbox{blue}{dx}";
    public const string IntegralColorBoxWrong = @"\colorbox{red}\int^\colorbox{yellow}\infty_\colorbox{purple}{-\infty}\colorbox{green}x\ \colorbox{blue}{dx}";
    public const string IntegralScripts = @"\int\int\int^{\infty}\int_0\int^{\infty}_0\int";
    public const string ItalicAlignment = @"\colorbox{yellow}P\\\begin{array}{r}\colorbox{yellow}{PF}\\\colorbox{yellow}F\end{array}";
    public const string ItalicScripts = @"U_3^2UY_3^2U_3Y^2f_1f^2ff";

    public const string KetSum = @"\frac{1}{\sqrt{2^n}} \sum_{i=0}^{2^n-1} \Ket{i}";

    public const string LargeBra = @"\Bra{\frac{a}{2}+\frac{b}{3}}";
    public const string LargeKet = @"\Ket{\frac{a}{2}+\frac{b}{3}}";
    public const string LargerDelimiters = @"\left(\left[\left\{\left(\left[\left\{\left(\left[\left\{\left(\left[\left\{\square\right\}^\square\right]^\square\right)^\square\right\}^\square\right]^\square\right)^\square\right\}^\square\right]^\square\right)^\square\right\}^\square\right]^\square\right)^\square";
    public const string LeftRight = @"\left(\frac23\right)";
    public const string LeftRightMinus = @"\left(\frac23\right)-";
    public const string LeftSide = @"\frac{1}{\left(\sqrt{\phi \sqrt{5}}-\phi\right) e^{\frac25 \pi}}";
    public const string LineStyles = @"a \displaystyle a \textstyle a \scriptstyle a \scriptscriptstyle a";
    public const string LnEquation = @"\ln(P) = a_0 - \frac{a_1}{a_2 + T}";
    public const string Logic = @"\neg(P\land Q) \iff (\neg P)\lor(\neg Q)";

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
    public const string Matrix0 = @"\begin{pmatrix}\end{pmatrix}";
    public const string Matrix1 = @"\begin{pmatrix}2\end{pmatrix}";
    public const string Matrix2 = @"\begin{pmatrix}a&b\\c&d\end{pmatrix}";
    public const string Matrix3 = @"\begin{bmatrix}a&b\\c&d\\e&f\end{bmatrix}";
    public const string Matrix4 = @"\begin{bmatrix}a&b\\c&d\\e&f\\g&h\end{bmatrix}";
    public const string Matrix5 = @"\begin{Bmatrix}a&b\\c&d\\e&f\\g&h\\i&j\end{Bmatrix}";
    public const string Matrix6 = @"\begin{vmatrix}a&b\\c&d\\e&f\\g&h\\i&j\\k&l\end{vmatrix}";
    public const string Matrix7 = @"\begin{Vmatrix}a&b\\c&d\\e&f\\g&h\\i&j\\k&l\\m&n\end{Vmatrix}";
    public const string Matrixception = @"\begin{Vmatrix}\begin{vmatrix}\begin{Bmatrix}\begin{bmatrix}\begin{pmatrix}a&b\\c&d\end{pmatrix}\end{bmatrix}\end{Bmatrix}\end{vmatrix}\end{Vmatrix}";
    //TODO: Uncomment when \middle is implemented
    //public const string MiddleDelimiter = @"A = \left\{ \frac{x_i}{i} \middle| i\in \mathcal{I} \right\}";

    public const string Nothing = @"";
    public const string Overline = @"\overline{Overline}";
    public const string Pi = @"\pi";
    public const string Phi = @"\phi";

    public const string QuadraticFormula = @"x = -b \pm \frac{\sqrt{b^2-4ac}}{2a}";
    // Solutions of x for x^4+4x^3+16x^2+4x+1=y
    public const string QuarticSolutions = @"\left\{-1+\frac{-\sqrt{10+2\times \left(\frac{-25}{3}+\sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-\right)\right)}^{3}}{27}}}
+\frac{-\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}{3\times \sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-\right)\right)}^{3}}{27}}}
}\right)}-\sqrt{-\left(30+2\times\left(\frac{-25}{3}+\sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-\right)\right)}^{3}}{27}}}
+\frac{-\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}{3\times \sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-\right)\right)}^{3}}{27}}}
}\right)+\frac{40}{\sqrt{10+2\times \left(\frac{-25}{3}+\sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times\left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-\right)\right)}^{3}}{27}}}
+\frac{-\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}{3\times \sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}}\right)}}\right)}}{2},\\
-1+\frac{-\sqrt{10+2\times \left(\frac{-25}{3}+\sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
+\frac{-\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}{3\times \sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
}\right)}+\sqrt{-\left(30+2\times\left(\frac{-25}{3}+\sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
+\frac{-\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}{3\times \sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
}\right)+\frac{40}{\sqrt{10+2\times \left(\frac{-25}{3}+\sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
+\frac{-\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}{3\times \sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}}\right)}}\right)}}{2},\\
-1+\frac{\sqrt{10+2\times \left(\frac{-25}{3}+\sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
+\frac{-\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}{3\times \sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
}\right)}-\sqrt{-\left(30+2\times \left(\frac{-25}{3}+\sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
+\frac{-\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}{3\times \sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
}\right)+\frac{-40}{\sqrt{10+2\times \left(\frac{-25}{3}+\sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
+\frac{-\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}{3\times \sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}}\right)}}\right)}}{2},\\
-1+\frac{\sqrt{10+2\times \left(\frac{-25}{3}+\sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times\left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
+\frac{-\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}{3\times \sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times\left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
}\right)}+\sqrt{-\left(30+2\times \left(\frac{-25}{3}+\sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
+\frac{-\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}{3\times \sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
}\right)+\frac{-40}{\sqrt{10+2\times \left(\frac{-25}{3}+\sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}
+\frac{-\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}{3\times \sqrt[3]{\frac{-\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}{2}+\sqrt{\frac{{\left(\frac{-250}{27}+\frac{10\times \left(9+1-y_{}\right)}{3}-50\right)}^{2}}{4}+\frac{{\left(\frac{-25}{3}-\left(9+1-y_{}\right)\right)}^{3}}{27}}}}\right)}}\right)}}{2}\right\}";

    public const string Radical = @"\sqrt{3}";
    public const string RadicalNested = @"\sqrt{\sqrt{x}}";
    public const string RadicalNestedDeep = @"\sqrt{\sqrt{\sqrt{\sqrt{\sqrt{\sqrt{\sqrt{\sqrt{x}}}}}}}}";
    public const string RadicalSum = @"2 + \sqrt{3}";
    public const string RadicalFraction = @"2+ \frac{\sqrt{3}}{2}";
    public const string RadicalPower = @"\sqrt{2}^\sqrt{3}";
    public const string RaiseBox = @"a\raisebox{1mu}a\raisebox{2mu}a\raisebox{3mu}a\raisebox{4mu}a";
    public const string RightSide = @"1+\frac{e^{-2\pi}} {1 +\frac{e^{-4\pi}} {1+\frac{e^{-6\pi}} {1+\frac{e^{-8\pi}} {1+\cdots} } } }";

    public const string SolveEquations = @"\text{Solve } \begin{cases} y=x^2-x+3 \\ y=x^2+\sqrt x-\frac2x \end{cases} \text{ using the graphical method.}";
    public const string SomeLimit = @"\lim_{x\to\infty}\frac{e^2}{1-x}=\limsup_{\sigma}5";
    public const string SimpleLimit = @"\lim_{x\to\infty}3=3";
    public const string SimpleShortProof = @"\begin{aligned}&\because x+3=5\\&\therefore x=2\end{aligned}";
    public const string ShortIntegral = @"\int_0^1";
    public const string SummationWithCup = @"\sum_{n=1}^{\infty}\frac{1+n}{1-n}=\bigcup_{1}C\cup B";
    public const string SummationDouble = @"\sum \sum";
    public const string SummationBigCup = @"234 \bigcup_1";
    public const string SummationWithBigLimits = @"\sum^{4^{5^{6^{7^{8^{9^{10^{11^{12^{13}}}}}}}}}}_{i=3_{2_{1_{0_{-1_{-2_{-3}}}}}}}i";
    public const string SummationWithLimits = @"\sum_{n=1}^{\infty}";

    public const string TangentPeriodShift = @"\tan(\theta\pm\frac\pi4)=\frac{\tan\theta\pm1}{1\mp\tan\theta}";
    public const string Taylor = @"\begin{eqnarray} e^x  &=&  \sum_{n=0}^{\infty}\frac{x^n}{n!} \\ \\ \sin(x) &=& \sum_{n=0}^{\infty}(-1)^n\frac{x^{2n+1}}{(2n+1)!}  \\ \\ -\ln(1-x)   &=& \sum_{n=1}^{\infty}\frac{x^n}{n}  \ \ \ \ \ (-1 \leq x < 1) \end{eqnarray}";
    public const string TwoSin = @"2 \sin";

    public const string Underline = @"\underline{Underline}";

    public const string VectorProjection = @"Proj_\vec{v}\vec{u}=|\vec u|\cos\theta\times\frac\vec v{|\vec v|}=|\vec u|\frac{\vec u \cdot \vec v}{|\vec u||\vec v|}\times\frac\vec v{|\vec v|}\\\text{Suppose \mathit{u} and \mathit v are unit vectors, }Proj_\vec v\vec u = (\vec u\cdot\vec v)\vec v";
  }
}