namespace CSharpMath.Rendering.Tests {
  public sealed class TestRenderingTextData : TestRenderingSharedData<TestRenderingTextData> {
    public const string Accent = @"\'a";
    public const string DisplayMath = @"$$\int_{a_1^2}^{a_2^2}\sqrt\frac x2dx$$";
    public const string FontStyles = @"\textnormal F\textrm F\textbf F\textcal F\texttt F\textit F\textsf F\textfrak F\textbb F\textbfit F";
    public const string InlineMath = @"$\int_{a_1^2}^{a_2^2}\sqrt\frac x2dx$";
    public const string IntegrationByParts = @"
        If $u = u(x)$ and \(du = u'(x)\:dx\), while \(v=v(x)\) and
        $dv = v'(x)\:dx$, then integration by parts states that:$$
        \begin{array}{rcl}
            \int^b_au(x)v'(x)\:dx &=& \left[ u(x)v(x) \right]^b_a - \int^b_a u'(x)v(x)\:dx \\
                                  &=& u(b)v(b) - u(a)v(a) - \int^b_a u'(x)v(x)\:dx
        \end{array} $$ or more compactly: \[\int u\:dv = uv - \int v \:du.\]";
    public const string KindergartenQuestion = @"Mary has \$10. Now she has \$20. How much did she earn?";
    // TODO: Fix plz
    //public const string MatrixLookalike = @"$$x$$ $$y$$ $$z$$";
    //public const string MultilineDisplayMath = @"$$x$$\;$$y$$\;$$z$$";
    public const string PunctuationAffectsLineBreaking = @"11111222223333344444555556666677777888889999900000 12345678.\\11111222223333344444555556666677777888889999900000 123456789.\\11111222223333344444555556666677777888889999900000 \(\overline{12345678}$.\\11111222223333344444555556666677777888889999900000 $\overline{123456789}\).";
    public const string QuadraticPolynomial = @"$$p(x)=ax^2+bx+c, \text{ where } a \neq 0$$ Above is the definition of a quadratic ($2^{nd}$ degree) polynomial.";
    public const string WideDisplayMaths = @"Text 1$$\sum\int^3_2x\ dx$$Text 2";
  }
}
