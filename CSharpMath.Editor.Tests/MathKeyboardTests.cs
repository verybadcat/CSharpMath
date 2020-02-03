using System;
using CSharpMath.FrontEnd;
using CSharpMath.Tests.FrontEnd;
using Xunit;
using T = Xunit.InlineDataAttribute; // 'T'est
using K = CSharpMath.Editor.MathKeyboardInput; // 'K'ey

namespace CSharpMath.Editor.Tests {
  using EventInteractor = Action<MathKeyboard<TestFont, char>, EventHandler>;
  public class MathKeyboardTests {
    private static readonly TypesettingContext<TestFont, char> context = TestTypesettingContexts.Instance;
    static void Test(string latex, K[] inputs) {
      var keyboard = new MathKeyboard<TestFont, char>(context);
      keyboard.KeyPress(inputs);
      Assert.Equal(latex, keyboard.LaTeX);
    }
    static void TestEvent(EventInteractor attach, EventInteractor detach, K[] inputs) {
      var keyboard = new MathKeyboard<TestFont, char>(context);
      Assert.Raises<EventArgs>(
        h => attach(keyboard, new EventHandler(h)),
        h => detach(keyboard, new EventHandler(h)),
        () => keyboard.KeyPress(inputs));
    }

    // Copy for more test categories
    [
      Theory,
      T(@""),
    ]
    public void Empty(string latex, params K[] inputs) => Test(latex, inputs);

    [
      Theory,
      T(@"1", K.D1),
      T(@"x", K.SmallX),
      //Brackets
      T(@"()[]\{ \} ", K.LeftRoundBracket, K.RightRoundBracket, K.LeftSquareBracket, K.RightSquareBracket,
        K.LeftCurlyBracket, K.RightCurlyBracket),
      //Decimals
      T(@"0123456789.", K.D0, K.D1, K.D2, K.D3, K.D4, K.D5, K.D6, K.D7, K.D8, K.D9, K.Decimal),
      //Basic operators
      T(@"+--\times \times \div ::\% ,!\infty \angle \degree \vert \log \ln ",
        K.Plus, K.Minus, K.Minus_, K.Multiply, K.Multiply_, K.Divide, K.Ratio, K.Ratio_, K.Percentage, 
       K.Comma, K.Factorial, K.Infinity, K.Angle, K.Degree, K.VerticalBar, K.Logarithm, K.NaturalLogarithm),
      //Relations
      T(@"=\neq <\leq >\geq ", K.Equals, K.NotEquals, K.LessThan, K.LessOrEquals, K.GreaterThan, K.GreaterOrEquals),
      //Capital English alphabets
      T(@"ABCDEFGHIJKLMNOPQRSTUVWXYZ", K.A, K.B, K.C, K.D, K.E, K.F, K.G, K.H, K.I, K.J,
        K.K, K.L, K.M, K.N, K.O, K.P, K.Q, K.R, K.S, K.T, K.U, K.V, K.W, K.X, K.Y, K.Z),
      //Small English alphabets
      T(@"abcdefghijklmnopqrstuvwxyz", K.SmallA, K.SmallB, K.SmallC, K.SmallD, K.SmallE, K.SmallF, K.SmallG, 
        K.SmallH, K.SmallI, K.SmallJ, K.SmallK, K.SmallL, K.SmallM, K.SmallN, K.SmallO, K.SmallP, K.SmallQ, 
        K.SmallR, K.SmallS, K.SmallT, K.SmallU, K.SmallV, K.SmallW, K.SmallX, K.SmallY, K.SmallZ),
      //Capital Greek alphabets
      T(@"ΑΒ\Gamma \Delta ΕΖΗ\Theta ΙΚ\Lambda ΜΝ\Xi Ο\Pi Ρ\Sigma Τ\Upsilon \Phi Χ\Omega ",
        K.Alpha, K.Beta, K.Gamma, K.Delta, K.Epsilon, K.Zeta, K.Eta, K.Theta, 
        K.Iota, K.Kappa, K.Lambda, K.Mu, K.Nu, K.Xi, K.Omicron, 
        K.Pi, K.Rho, K.Sigma, K.Tau, K.Upsilon, K.Phi, K.Chi, K.Omega),
      //Small Greek alphabets
      T(@"\alpha \beta \gamma \delta \varepsilon \zeta \eta \theta \iota \kappa \lambda \mu " +
        @"\nu \xi \omicron \pi \rho \sigma \varsigma \tau \upsilon \varphi \chi \omega ",
        K.SmallAlpha, K.SmallBeta, K.SmallGamma, K.SmallDelta, K.SmallEpsilon,
        K.SmallZeta, K.SmallEta, K.SmallTheta, K.SmallIota, K.SmallKappa,
        K.SmallLambda, K.SmallMu, K.SmallNu, K.SmallXi, K.SmallOmicron,
        K.SmallPi, K.SmallRho, K.SmallSigma, K.SmallSigma2, K.SmallTau,
        K.SmallUpsilon, K.SmallPhi, K.SmallChi, K.SmallOmega),
      //Trigonometric functions
      T(@"\sin \cos \tan \cot \sec \csc \arcsin \arccos \arctan \arccot \arcsec \arccsc ",
        K.Sine, K.Cosine, K.Tangent, K.Cotangent, K.Secant, K.Cosecant,
        K.ArcSine, K.ArcCosine, K.ArcTangent, K.ArcCotangent, K.ArcSecant, K.ArcCosecant),
      //Hyperbolic functions
      T(@"\sinh \cosh \tanh \coth \sech \csch \arsinh \arcosh \artanh \arcoth \arsech \arcsch ",
        K.HyperbolicSine, K.HyperbolicCosine, K.HyperbolicTangent,
        K.HyperbolicCotangent, K.HyperbolicSecant, K.HyperbolicCosecant,
        K.AreaHyperbolicSine, K.AreaHyperbolicCosine, K.AreaHyperbolicTangent,
        K.AreaHyperbolicCotangent, K.AreaHyperbolicSecant, K.AreaHyperbolicCosecant),
      T(@"X_{2_3}", K.X, K.Subscript, K.D2, K.Subscript, K.D3),
      T(@"x^{\frac{2}{■}}", K.SmallX, K.Power, K.D2, K.Slash),
      // https://github.com/verybadcat/CSharpMath/issues/39
      T(@"x^{\frac{123}{■}}", K.SmallX, K.Power, K.D1, K.D2, K.D3, K.Slash),
      T(@"\frac{1}{■}", K.Slash),
      // https://github.com/kostub/MathEditor/issues/18
      T(@"\frac{4}{\frac{4}{■}}", K.D4, K.Slash, K.D4, K.Slash),
      T(@"\square ^{\square ^{\square ^■}}", K.Power, K.Power, K.Power),
      T(@"e^■", K.SmallE, K.Power),
      T(@"e^■", K.BaseEPower),
      T(@"\sqrt{3}", K.SquareRoot, K.D3),
      T(@"\sqrt[3]{3}", K.CubeRoot, K.D3),
      // https://github.com/verybadcat/CSharpMath/issues/47
      T(@"2^■", K.D2, K.Power),
      T(@"3+\square ^■", K.D3, K.Plus, K.Power),
      T(@"[\square ^■", K.LeftSquareBracket, K.Power),
      T(@")^■", K.RightRoundBracket, K.Power),
      T(@"\sin ^■", K.Sine, K.Power),
      T(@"\infty ^■", K.Infinity, K.Power),
      T(@"\log _■", K.Logarithm, K.Subscript),
      T(@"\log _■", K.LogarithmWithBase),
      T(@"\log _3", K.LogarithmWithBase, K.D3),
    ]
    public void AtomInput(string latex, params K[] inputs) => Test(latex, inputs);

    [
      Theory,
      T(@"", K.Left, K.Left, K.Left, K.Right, K.Right, K.Right),
      T(@"\square ^■2", K.Power, K.Left, K.Left, K.Right, K.Right, K.Right, K.D2, K.Left, K.Left),
      T(@"+-\times \div ", K.Divide, K.Left, K.Multiply, K.Left, K.Minus, K.Left, K.Plus),
      T(@"\sin \cos \tan \arcsin \arccos \arctan ", K.ArcSine, K.ArcCosine, K.Left, K.Left,
        K.Sine, K.Cosine, K.Right, K.Right, K.ArcTangent, K.Left, K.Left, K.Left, K.Tangent),
      T(@"e^{\square }", K.Power, K.Left, K.SmallE, K.Right),
      T(@"e^■", K.Power, K.Left, K.SmallE, K.Left),
      T(@"\vert x\vert \vert y\vert ", K.Absolute, K.SmallX, K.Right, K.VerticalBar, K.SmallY, K.VerticalBar),
      T(@"(1)(2)", K.BothRoundBrackets, K.D1, K.Right, K.LeftRoundBracket, K.D2, K.RightRoundBracket),
      T(@"\sqrt{\sqrt[4]{3}}", K.SquareRoot, K.NthRoot, K.D4, K.Right, K.D3),
      T(@"23^{\square }", K.D2, K.Power, K.Left, K.D3),
      T(@"2^{\square }4", K.D2, K.Power, K.Right, K.D4),
      T(@"\sin \Pi ^2", K.Sine, K.Power, K.D2, K.Left, K.Left, K.Pi),
      T(@"\frac{23}{4}_6^578", K.Fraction, K.D3, K.Right, K.D4, K.Right, K.Power, K.D5, K.Right, K.Subscript, 
        K.D6, K.Right, K.D7, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left,
        K.D2, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.D8),
      T(@"\sqrt[23]{4}_6^578", K.NthRoot, K.D3, K.Right, K.D4, K.Right, K.Power, K.D5, K.Right, K.Subscript,
        K.D6, K.Right, K.D7, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left,
        K.D2, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.D8),
      T(@"1\frac{\square }{\square }_{\square }^{\square }90", K.Fraction, K.Right, K.Right, K.Power, K.Right,
        K.Subscript, K.Right, K.D9, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.D1,
        K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.D0),
      T(@"1\sqrt[\square ]{\square }_{\square }^{\square }90", K.NthRoot, K.Right, K.Right, K.Power, K.Right,
        K.Subscript, K.Right, K.D9, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.D1,
        K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.D0),
      T(@"01\frac{\square }{\square }_{\square }^{\square }90", K.D0, K.Fraction, K.Right, K.Right, K.Power, K.Right,
        K.Subscript, K.Right, K.D9, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.D1,
        K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.D0),
      T(@"01\sqrt[\square ]{\square }_{\square }^{\square }90", K.D0, K.NthRoot, K.Right, K.Right, K.Power, K.Right,
        K.Subscript, K.Right, K.D9, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.D1,
        K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.D0),
    ]
    public void LeftRightNavigation(string latex, params K[] inputs) => Test(latex, inputs);

    [
      Theory,
      T(@"", K.Clear, K.Clear, K.Left, K.Left, K.X, K.Clear, K.Right, K.Right, K.Y, K.Clear),
      T(@"1", K.D2, K.D3, K.Clear, K.D1),
      T(@"2", K.Slash, K.Slash, K.Slash, K.Fraction, K.NthRoot, K.CubeRoot, K.Clear, K.Left, K.D2),
      T(@"3", K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.Clear, K.D3),
    ]
    public void Clear(string latex, params K[] inputs) => Test(latex, inputs);

    [
      Theory,
      T(K.Dismiss),
      T(K.Clear, K.Clear, K.Left, K.Left, K.Dismiss, K.X, K.Clear, K.Right, K.Right, K.Y, K.Clear),
      T(K.Dismiss, K.D2, K.D3, K.Clear, K.D1, K.Dismiss),
      T(K.Slash, K.Slash, K.Slash, K.Fraction, K.NthRoot, K.CubeRoot, K.Clear, K.Left, K.D2, K.Dismiss),
      T(K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.Dismiss),
    ]
    public void Dismiss(params K[] inputs) =>
      TestEvent((k, h) => k.DismissPressed += h, (k, h) => k.DismissPressed -= h, inputs);
    [
      Theory,
      T(K.Return),
      T(K.Clear, K.Clear, K.Left, K.Left, K.Return, K.X, K.Clear, K.Right, K.Right, K.Y, K.Clear),
      T(K.Return, K.D2, K.D3, K.Clear, K.D1, K.Return),
      T(K.Slash, K.Slash, K.Slash, K.Fraction, K.NthRoot, K.CubeRoot, K.Clear, K.Left, K.D2, K.Return),
      T(K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.D1, K.Return),
    ]
    public void Return(params K[] inputs) =>
      TestEvent((k, h) => k.ReturnPressed += h, (k, h) => k.ReturnPressed -= h, inputs);

    [
      Theory,
      T(@"", K.Backspace, K.Backspace, K.Backspace, K.Backspace, K.Backspace),
      T(@"1", K.D1, K.D2, K.Backspace),
      T(@"x^2", K.SmallX, K.Power, K.D2, K.D1, K.Backspace),
      T(@"y_{3_4}", K.SmallY, K.Subscript, K.D3, K.Subscript, K.Backspace, K.Backspace, K.D4, K.D5, K.Backspace),
      T(@"5^■", K.D5, K.Power, K.Iota, K.Kappa, K.SmallEta, K.Backspace, K.Backspace, K.Backspace, K.Backspace),
      T(@"\frac{■}{\square }", K.Fraction, K.Backspace),
      T(@"\vert \vert ", K.Absolute, K.Absolute, K.Backspace, K.Backspace, K.Backspace),
      T(@"(())))", K.BothRoundBrackets, K.BothRoundBrackets, K.BothRoundBrackets, K.BothRoundBrackets,
        K.Backspace, K.Backspace),
    ]
    public void Backspace(string latex, params K[] inputs) => Test(latex, inputs);

    [
      Theory,
      T(@"", K.Left, K.Left, K.Backspace, K.Backspace, K.Right, K.Right, K.Backspace, K.Backspace, K.Left),
      T(@"\frac{\square }{3}", K.Slash, K.D3, K.Left, K.Left, K.Backspace, K.Left),
      T(@"1_3", K.D1, K.D2, K.Subscript, K.D3, K.Left, K.Left, K.Backspace),
      T(@"1_3^2", K.D1, K.D4, K.Subscript, K.D3, K.Left, K.Left, K.Power, K.D2, K.Left, K.Left, K.Backspace),
      T(@"1_2^3", K.D1, K.D4, K.Power, K.D3, K.Left, K.Left, K.Subscript, K.D2, K.Left, K.Left, K.Left, K.Left, K.Backspace),
      T(@"■^6", K.Power, K.D6, K.Left, K.Left, K.Left, K.X, K.Left, K.Left, K.Left, K.Backspace),
      T(@"\sqrt[■]{\square }", K.NthRoot, K.SmallA, K.Backspace),
      T(@"\sqrt{■}", K.SquareRoot, K.SmallA, K.Backspace),
      T(@"\frac{1}{■}", K.Slash, K.D6, K.Backspace),
      T(@"■_5", K.Subscript, K.D5, K.Left, K.Left, K.Backspace, K.X, K.Left, K.Left, K.Left, K.Backspace),
      T(@"7+1^X", K.D7, K.Plus, K.D1, K.D2, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"7.^X", K.D7, K.Decimal, K.D1, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"7+■^X", K.D7, K.Plus, K.D1, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"7-■^X", K.D7, K.Minus, K.D1, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"7\times ■^X", K.D7, K.Multiply, K.D1, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"7\div ■^X", K.D7, K.Divide, K.D1, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"7:■^X", K.D7, K.Ratio, K.D1, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"7=■^X", K.D7, K.Equals, K.D1, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"7\neq ■^X", K.D7, K.NotEquals, K.D1, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"7<■^X", K.D7, K.LessThan, K.D1, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"7\leq ■^X", K.D7, K.LessOrEquals, K.D1, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"7>■^X", K.D7, K.GreaterThan, K.D1, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"7\geq ■^X", K.D7, K.GreaterOrEquals, K.D1, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"7;■^X", K.D7, K.Semicolon, K.D1, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"7,■^X", K.D7, K.Comma, K.D1, K.Power, K.X, K.Left, K.Left, K.Backspace),
      T(@"a(c-2)^3", K.SmallA, K.Power, K.D3, K.Left, K.Left, K.BothRoundBrackets, K.SmallC, K.Minus, K.D2),
    ]
    public void LeftRightBackspace(string latex, params K[] inputs) => Test(latex, inputs);

    [Theory, T(@"\square ^■", K.Power), T(@"\square _■", K.Subscript)]
    public void ScriptsAtBeginningOfLine(string latex, params K[] inputs) => Test(latex, inputs);

    [
      Theory,
      T(@"eA^{\square }", K.BaseEPower, K.Left, K.A),
      T(@"e\frac{■}{\square }^{\square }", K.BaseEPower, K.Left, K.Fraction),
      T(@"e\sqrt[3]{■}^{\square }", K.BaseEPower, K.Left, K.CubeRoot),
      T(@"eAB^{\square }", K.BaseEPower, K.Left, K.A, K.B),
      T(@"eA\frac{\square }{\square }\sqrt{\square }B^{\square }",
        K.BaseEPower, K.Left, K.A, K.Fraction, K.Right, K.Right, K.SquareRoot, K.Right, K.B),
      T(@"eA\frac{\square }{\square }\sqrt[3]{\square }B_{\square }",
        K.SmallE, K.Subscript, K.Left, K.A, K.Fraction, K.Right, K.Right, K.CubeRoot, K.Right, K.B),
      T(@"eA\frac{\square }{\square }\sqrt[\square ]{\square }B_{\square }^{\square }",
        K.BaseEPower, K.Left, K.Subscript, K.Left, K.Left, K.A, K.Fraction, K.Right, K.Right, K.NthRoot, K.Right, K.Right, K.B),
    ]
    public void BetweenBaseAndScriptsInsert(string latex, params K[] inputs) => Test(latex, inputs);

    [
      Theory,
      T(@"eA\frac{\square }{\square }\sqrt[3]{\square }^{\square }",
        K.BaseEPower, K.Left, K.A, K.Fraction, K.Right, K.Right, K.CubeRoot, K.Right, K.B,
        K.Backspace),
      T(@"e^{\square }",
        K.BaseEPower, K.Left, K.A, K.Fraction, K.Right, K.Right, K.SquareRoot, K.Right, K.B,
        K.Backspace, K.Backspace, K.Backspace, K.Backspace),
    ]
    public void BetweenBaseAndScriptsRemove(string latex, params K[] inputs) => Test(latex, inputs);

    [
      Theory,
      T(@"\frac{1}{■}", K.Slash),
      T(@"\frac{1}{■}", K.D1, K.Slash),
      T(@"\frac{2}{■}", K.D2, K.Slash),
      T(@"\frac{12}{■}", K.D1, K.D2, K.Slash),
      T(@"\frac{a}{■}", K.SmallA, K.Slash),
      T(@"\frac{XyZ}{■}", K.X, K.SmallY, K.Z, K.Slash),
      T(@"\frac{\alpha \beta c}{■}", K.SmallAlpha, K.SmallBeta, K.SmallC, K.Slash),
      T(@"\frac{\infty }{■}", K.Infinity, K.Slash),
      T(@"\frac{\sin ^2\theta }{■}", K.Sine, K.Power, K.D2, K.Right, K.SmallTheta, K.Slash),
      T(@"\frac{\log _3\pi }{■}", K.LogarithmWithBase, K.D3, K.Right, K.SmallPi, K.Slash),
      
      T(@"\frac{1}{\frac{1}{■}}", K.Slash, K.Slash),
      T(@"\frac{1}{\frac{2}{■}}", K.Slash, K.D2, K.Slash),
      T(@"\frac{1}{\square }\times \frac{1}{■}", K.Slash, K.Right, K.Slash),
      T(@"\frac{1}{2}\times \frac{1}{■}", K.Slash, K.D2, K.Right, K.Slash),
      
      T(@"\sqrt{\frac{2}{■}}", K.SquareRoot, K.D2, K.Slash),
      T(@"\frac{\sqrt{2}}{■}", K.SquareRoot, K.D2, K.Right, K.Slash),
      T(@"\sqrt[\frac{1}{■}]{\square }", K.NthRoot, K.Slash),
      T(@"\log _{\frac{1}{■}}", K.LogarithmWithBase, K.Slash),
      
      T(@"\frac{1^{\square }}{■}", K.D1, K.Power, K.Left, K.Slash),
      T(@"\frac{123^{\square }}{■}", K.D1, K.D2, K.D3, K.Power, K.Left, K.Slash),
      T(@"\frac{x\infty 1^{\square }}{■}", K.SmallX, K.Infinity, K.D1, K.Power, K.Left, K.Slash),
      T(@"\frac{1_{\square }}{■}", K.D1, K.Subscript, K.Left, K.Slash),
      T(@"\frac{123_{\square }}{■}", K.D1, K.D2, K.D3, K.Subscript, K.Left, K.Slash),
      T(@"\frac{x\infty 1_{\square }}{■}", K.SmallX, K.Infinity, K.D1, K.Subscript, K.Left, K.Slash),
      
      T(@"\frac{()}{■}", K.BothRoundBrackets, K.Right, K.Slash),
      T(@"(\frac{()}{■}", K.LeftRoundBracket, K.BothRoundBrackets, K.Right, K.Slash),
      T(@"(\frac{1}{■})", K.BothRoundBrackets, K.Slash),
      T(@"\frac{(\frac{1}{\square })}{■}", K.BothRoundBrackets, K.Slash, K.Right, K.Right, K.Slash),
      T(@"(\frac{[\} }{■}", K.LeftRoundBracket, K.LeftSquareBracket, K.RightCurlyBracket, K.Slash),
      T(@"\{ \frac{[0,\infty )}{■}",
        K.LeftCurlyBracket, K.LeftSquareBracket, K.D0, K.Comma, K.Infinity, K.RightRoundBracket, K.Slash),
      T(@"\frac{(\{ \} )([])}{■}",
        K.LeftRoundBracket, K.LeftCurlyBracket, K.RightCurlyBracket, K.RightRoundBracket,
        K.LeftRoundBracket, K.LeftSquareBracket, K.RightSquareBracket, K.RightRoundBracket, K.Slash),
      T(@"(\frac{(\{ \} )([])}{■}", K.LeftRoundBracket,
        K.LeftRoundBracket, K.LeftCurlyBracket, K.RightCurlyBracket, K.RightRoundBracket,
        K.LeftRoundBracket, K.LeftSquareBracket, K.RightSquareBracket, K.RightRoundBracket, K.Slash),
      
      T(@"\frac{(1+2)}{■}", K.BothRoundBrackets, K.D1, K.Plus, K.D2, K.Right, K.Slash),
      T(@"1+\frac{2}{■}", K.D1, K.Plus, K.D2, K.Slash),
      T(@"1-\frac{2}{■}", K.D1, K.Minus, K.D2, K.Slash),
      T(@"1\times \frac{2}{■}", K.D1, K.Multiply, K.D2, K.Slash),
      T(@"1\times \frac{2}{■}", K.D1, K.Multiply_, K.D2, K.Slash),
      T(@"1\div \frac{2}{■}", K.D1, K.Divide, K.D2, K.Slash),
      T(@"1:\frac{2}{■}", K.D1, K.Ratio, K.D2, K.Slash),
      T(@"1:\frac{2}{■}", K.D1, K.Ratio_, K.D2, K.Slash),
      T(@"1=\frac{2}{■}", K.D1, K.Equals, K.D2, K.Slash),
      T(@"1\neq \frac{2}{■}", K.D1, K.NotEquals, K.D2, K.Slash),
      T(@"1<\frac{2}{■}", K.D1, K.LessThan, K.D2, K.Slash),
      T(@"1\leq \frac{2}{■}", K.D1, K.LessOrEquals, K.D2, K.Slash),
      T(@"1>\frac{2}{■}", K.D1, K.GreaterThan, K.D2, K.Slash),
      T(@"1\geq \frac{2}{■}", K.D1, K.GreaterOrEquals, K.D2, K.Slash),
      T(@"\frac{1}{\frac{2}{■}}", K.D1, K.Slash, K.D2, K.Slash),
      T(@"\sqrt{x+\frac{2}{■}}", K.SquareRoot, K.SmallX, K.Plus, K.D2, K.Slash),
      T(@"\frac{(x+\sqrt{2})}{■}", K.BothRoundBrackets, K.SmallX, K.Plus, K.SquareRoot, K.D2, K.Right, K.Right, K.Slash),
      T(@"\sqrt[X2Z+\frac{X2Z}{■}]{\square }", K.NthRoot, K.X, K.D2, K.Z, K.Plus, K.X, K.D2, K.Z, K.Slash),
    ]
    public void Slash(string latex, params K[] inputs) => Test(latex, inputs);
  }
}