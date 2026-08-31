using System;
using System.Linq;
using CSharpMath.Display.FrontEnd;
using Xunit;
using EventInteractor = System.Action<CSharpMath.Editor.MathKeyboard<CSharpMath.Core.BackEnd.TestFont, System.Text.Rune>, System.EventHandler>;
using K = CSharpMath.Editor.MathKeyboardInput;
using T = Xunit.InlineDataAttribute;
using TGlyph = System.Text.Rune;

namespace CSharpMath.Core.EditorTests {
  using BackEnd;
  using Editor;
  public class KeyPressTests {
    private static readonly TypesettingContext<TestFont, TGlyph> context = TestTypesettingContext.Instance;
    static void Test(string latex, params K[] inputs) {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(context, new TestFont());
      keyboard.KeyPress(inputs);
      Assert.Equal(latex, keyboard.LaTeX);
    }
    static void Test(string latex, TestFont font, params K[] inputs) {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(context, font);
      keyboard.KeyPress(inputs);
      Assert.Equal(latex, keyboard.LaTeX);
    }
    static void TestVisual(MathKeyboardHorizontalNavigationMode mode, string latex, params K[] inputs) {
      var keyboard = VisualKeyboard(mode);
      keyboard.KeyPress(inputs);
      Assert.Equal(latex, keyboard.LaTeX);
    }
    static MathKeyboard<TestFont, TGlyph> VisualKeyboard(MathKeyboardHorizontalNavigationMode mode) =>
      new(context, new TestFont(10)) { HorizontalNavigationMode = mode };

    [Fact]
    public void HorizontalNavigationDefaultsToExhaustive() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(context, new TestFont());
      Assert.Equal(MathKeyboardHorizontalNavigationMode.Exhaustive, keyboard.HorizontalNavigationMode);
    }

    [Fact]
    public void VisualUpperFractionLeavesEitherRowAndReentersNumerator() =>
      TestVisual(MathKeyboardHorizontalNavigationMode.VisualUpper, @"\frac{13}{2}",
        K.D1, K.Slash, K.D2, K.Right, K.Left, K.D3);

    [Fact]
    public void VisualLowerFractionLeavesEitherRowAndReentersDenominator() =>
      TestVisual(MathKeyboardHorizontalNavigationMode.VisualLower, @"\frac{1}{23}",
        K.D1, K.Slash, K.D2, K.Right, K.Left, K.D3);

    [Theory]
    [InlineData(MathKeyboardHorizontalNavigationMode.VisualUpper, MathListSubIndexType.Numerator)]
    [InlineData(MathKeyboardHorizontalNavigationMode.VisualLower, MathListSubIndexType.Denominator)]
    public void VisualFractionEntryAndExitAreDirectional(
      MathKeyboardHorizontalNavigationMode mode, MathListSubIndexType preferredBranch) {
      var keyboard = VisualKeyboard(mode);
      keyboard.KeyPress(K.D1, K.Slash, K.D2);
      var fraction = Assert.IsType<CSharpMath.Atom.Atoms.Fraction>(Assert.Single(keyboard.MathList));
      var branchCount = preferredBranch == MathListSubIndexType.Numerator
        ? fraction.Numerator.Count : fraction.Denominator.Count;

      keyboard.InsertionIndex = new(0);
      keyboard.KeyPress(K.Right);
      Assert.Equal(new MathListIndex(0).LevelUpWithSubIndex(preferredBranch, 0), keyboard.InsertionIndex);
      keyboard.KeyPress(K.Left);
      Assert.Equal(new MathListIndex(0), keyboard.InsertionIndex);

      keyboard.InsertionIndex = new(1);
      keyboard.KeyPress(K.Left);
      Assert.Equal(new MathListIndex(0).LevelUpWithSubIndex(preferredBranch, branchCount), keyboard.InsertionIndex);
      keyboard.KeyPress(K.Right);
      Assert.Equal(new MathListIndex(1), keyboard.InsertionIndex);
    }

    [Theory]
    [InlineData(MathKeyboardHorizontalNavigationMode.VisualUpper, MathListSubIndexType.Numerator, MathListSubIndexType.Denominator, K.Down)]
    [InlineData(MathKeyboardHorizontalNavigationMode.VisualLower, MathListSubIndexType.Denominator, MathListSubIndexType.Numerator, K.Up)]
    public void VisualFractionOtherRowRemainsReachableVertically(
      MathKeyboardHorizontalNavigationMode mode, MathListSubIndexType preferredBranch,
      MathListSubIndexType otherBranch, K verticalInput) {
      var keyboard = VisualKeyboard(mode);
      keyboard.KeyPress(K.D1, K.Slash, K.D2);
      keyboard.InsertionIndex = new MathListIndex(0).LevelUpWithSubIndex(preferredBranch, 0);

      keyboard.KeyPress(verticalInput);

      Assert.Equal(otherBranch, keyboard.InsertionIndex.FinalSubIndexType);
      keyboard.InsertionIndex = new MathListIndex(0).LevelUpWithSubIndex(otherBranch, 0);
      keyboard.KeyPress(K.Left);
      Assert.Equal(new MathListIndex(0), keyboard.InsertionIndex);
    }

    [Theory]
    [InlineData(MathKeyboardHorizontalNavigationMode.VisualUpper, MathListSubIndexType.Superscript)]
    [InlineData(MathKeyboardHorizontalNavigationMode.VisualLower, MathListSubIndexType.Subscript)]
    public void VisualScriptPolicyChoosesPreferredScript(
      MathKeyboardHorizontalNavigationMode mode, MathListSubIndexType preferredBranch) {
      var keyboard = VisualKeyboard(mode);
      keyboard.KeyPress(K.SmallX, K.Subscript, K.D2, K.Right, K.Power, K.D3);

      keyboard.InsertionIndex = new(1);
      keyboard.KeyPress(K.Left);

      Assert.Equal(preferredBranch, keyboard.InsertionIndex.FinalSubIndexType);
    }

    [Theory]
    [InlineData(MathKeyboardHorizontalNavigationMode.VisualUpper, K.Subscript, MathListSubIndexType.Subscript)]
    [InlineData(MathKeyboardHorizontalNavigationMode.VisualLower, K.Power, MathListSubIndexType.Superscript)]
    public void VisualScriptPolicyFallsBackToOnlyScript(
      MathKeyboardHorizontalNavigationMode mode, K scriptInput, MathListSubIndexType availableBranch) {
      var keyboard = VisualKeyboard(mode);
      keyboard.KeyPress(K.SmallX, scriptInput, K.D2);
      keyboard.InsertionIndex = new(1);

      keyboard.KeyPress(K.Left);

      Assert.Equal(availableBranch, keyboard.InsertionIndex.FinalSubIndexType);
    }

    [Theory]
    [InlineData(MathKeyboardHorizontalNavigationMode.VisualUpper, MathListSubIndexType.Numerator, MathListSubIndexType.Superscript)]
    [InlineData(MathKeyboardHorizontalNavigationMode.VisualLower, MathListSubIndexType.Denominator, MathListSubIndexType.Subscript)]
    public void VisualCompoundRetainsIntrinsicAndScriptStages(
      MathKeyboardHorizontalNavigationMode mode, MathListSubIndexType intrinsicBranch,
      MathListSubIndexType scriptBranch) {
      var keyboard = VisualKeyboard(mode);
      keyboard.KeyPress(K.D1, K.Slash, K.D2, K.Right, K.Power, K.D3, K.Right, K.Subscript, K.D4);
      var fraction = Assert.IsType<CSharpMath.Atom.Atoms.Fraction>(Assert.Single(keyboard.MathList));
      var intrinsicCount = intrinsicBranch == MathListSubIndexType.Numerator
        ? fraction.Numerator.Count : fraction.Denominator.Count;
      keyboard.InsertionIndex = new MathListIndex(0).LevelUpWithSubIndex(intrinsicBranch, intrinsicCount);

      keyboard.KeyPress(K.Right);
      Assert.Equal(MathListSubIndexType.BetweenBaseAndScripts, keyboard.InsertionIndex.FinalSubIndexType);
      keyboard.KeyPress(K.Right);
      Assert.Equal(scriptBranch, keyboard.InsertionIndex.FinalSubIndexType);
    }
    static void TestEvent(EventInteractor attach, EventInteractor detach, K[] inputs) {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(context, new TestFont());
      Assert.Raises<EventArgs>(
        h => attach(keyboard, new EventHandler(h)),
        h => detach(keyboard, new EventHandler(h)),
        () => keyboard.KeyPress(inputs));
    }

    [Fact]
    public void NoDuplicateValues() {
      var names = Enum.GetNames(typeof(K));
      var values = (K[])Enum.GetValues(typeof(K));
      var duplicateValues =
        values
        .GroupBy(x => x)
        .Where(g => g.Count() > 1)
        .Select(g => $"({string.Join(" or ", names.Where(n => Enum.Parse<K>(n) == g.Key))}) == {(int)g.Key}")
        .ToArray();
      Assert.True(duplicateValues.Length == 0,
        $"{typeof(K).Name} has some duplicate values: {string.Join(", ", duplicateValues)}");
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
      T(@"+-\times \div :\% ,!\infty \angle \degree |\log \ln ",
        K.Plus, K.Minus, K.Multiply, K.Divide, K.Ratio, K.Percentage,
        K.Comma, K.Factorial, K.Infinity, K.Angle, K.Degree, K.VerticalBar, K.Logarithm, K.NaturalLogarithm),
      T(@"''\partial \leftarrow \uparrow \rightarrow \downarrow \  ",
        K.Prime, K.Prime, K.PartialDifferential, K.LeftArrow, K.UpArrow, K.RightArrow, K.DownArrow, K.Space),
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
      T(@"ΑΒ\Gamma \Delta ΕΖΗ\Theta ΙΚ\Lambda ΜΝ\Xi Ο\Pi Ρ\Sigma Τ\Upsilon \Phi Χ\Psi \Omega ",
        K.Alpha, K.Beta, K.Gamma, K.Delta, K.Epsilon, K.Zeta, K.Eta, K.Theta,
        K.Iota, K.Kappa, K.Lambda, K.Mu, K.Nu, K.Xi, K.Omicron,
        K.Pi, K.Rho, K.Sigma, K.Tau, K.Upsilon, K.Phi, K.Chi, K.Psi, K.Omega),
      //Small Greek alphabets
      T(@"\alpha \beta \gamma \delta \epsilon \varepsilon \zeta \eta \theta \iota \kappa \varkappa \lambda \mu " +
        @"\nu \xi \omicron \pi \varpi \rho \varrho \sigma \varsigma \tau \upsilon \phi \varphi \chi \psi \omega ",
        K.SmallAlpha, K.SmallBeta, K.SmallGamma, K.SmallDelta, K.SmallEpsilon, K.SmallEpsilon2,
        K.SmallZeta, K.SmallEta, K.SmallTheta, K.SmallIota, K.SmallKappa, K.SmallKappa2,
        K.SmallLambda, K.SmallMu, K.SmallNu, K.SmallXi, K.SmallOmicron, K.SmallPi,
      K.SmallPi2, K.SmallRho, K.SmallRho2, K.SmallSigma, K.SmallSigma2, K.SmallTau,
        K.SmallUpsilon, K.SmallPhi, K.SmallPhi2, K.SmallChi, K.SmallPsi, K.SmallOmega),
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
      //Large operators
      T(@"\sum \prod \int \iint \iiint \iiiint \oint \oiint \oiiint \intclockwise \varointclockwise \ointctrclockwise ",
        K.Summation, K.Product, K.Integral, K.DoubleIntegral, K.TripleIntegral, K.QuadrupleIntegral,
        K.ContourIntegral, K.DoubleContourIntegral, K.TripleContourIntegral,
        K.ClockwiseIntegral, K.ClockwiseContourIntegral, K.CounterClockwiseContourIntegral),
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
      T(@"\lim _■", K.LimitWithBase),
      T(@"\lim _3", K.LimitWithBase, K.D3),
      T(@"\int ^2", K.Integral, K.Power, K.D2),
      T(@"\int ^2", K.IntegralUpperLimit, K.D2),
      T(@"\int _2", K.Integral, K.Subscript, K.D2),
      T(@"\int _2", K.IntegralLowerLimit, K.D2),
      T(@"\int _2^{\square }", K.IntegralBothLimits, K.D2),
      T(@"\int ^{\square ^2}", K.IntegralUpperLimit, K.Power, K.D2),
      T(@"\int ^{\square _2}", K.IntegralUpperLimit, K.Subscript, K.D2),
      T(@"\int _{\square ^2}", K.IntegralLowerLimit, K.Power, K.D2),
      T(@"\int _{\square _2}", K.IntegralLowerLimit, K.Subscript, K.D2),
      T(@"\sum ^2", K.Summation, K.Power, K.D2),
      T(@"\sum ^2", K.SummationUpperLimit, K.D2),
      T(@"\sum _2", K.Summation, K.Subscript, K.D2),
      T(@"\sum _2", K.SummationLowerLimit, K.D2),
      T(@"\sum _2^{\square }", K.SummationBothLimits, K.D2),
      T(@"\sum ^{\square ^2}", K.SummationUpperLimit, K.Power, K.D2),
      T(@"\sum ^{\square _2}", K.SummationUpperLimit, K.Subscript, K.D2),
      T(@"\sum _{\square ^2}", K.SummationLowerLimit, K.Power, K.D2),
      T(@"\sum _{\square _2}", K.SummationLowerLimit, K.Subscript, K.D2),
      T(@"\prod ^2", K.Product, K.Power, K.D2),
      T(@"\prod ^2", K.ProductUpperLimit, K.D2),
      T(@"\prod _2", K.Product, K.Subscript, K.D2),
      T(@"\prod _2", K.ProductLowerLimit, K.D2),
      T(@"\prod _2^{\square }", K.ProductBothLimits, K.D2),
      T(@"\prod ^{\square ^2}", K.ProductUpperLimit, K.Power, K.D2),
      T(@"\prod ^{\square _2}", K.ProductUpperLimit, K.Subscript, K.D2),
      T(@"\prod _{\square ^2}", K.ProductLowerLimit, K.Power, K.D2),
      T(@"\prod _{\square _2}", K.ProductLowerLimit, K.Subscript, K.D2),
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
      T(@"\left| x\right| |y|", K.Absolute, K.SmallX, K.Right, K.VerticalBar, K.SmallY, K.VerticalBar),
      T(@"\left( 1\right) (2)", K.BothRoundBrackets, K.D1, K.Right, K.LeftRoundBracket, K.D2, K.RightRoundBracket),
      T(@"1\left( 2\left[ 3\left\{ ■\right\} \right] \right) ", K.BothRoundBrackets, K.BothSquareBrackets, K.BothCurlyBrackets, K.Left,
         K.D3, K.Left, K.Left, K.D2, K.Left, K.Left, K.D1, K.Left, K.Left, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right),
      T(@"\left( \left[ \left\{ ■\right\} 3\right] 2\right) 1", K.BothRoundBrackets, K.BothSquareBrackets, K.BothCurlyBrackets, K.Right,
         K.D3, K.Right, K.D2, K.Right, K.D1, K.Right, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left),
      T(@"\sqrt{\sqrt[4]{3}}", K.SquareRoot, K.NthRoot, K.D4, K.Right, K.D3),
      T(@"23^{\square }", K.D2, K.Power, K.Left, K.D3),
      T(@"2^{\square }4", K.D2, K.Power, K.Right, K.D4),
      T(@"\sin \Pi ^2", K.Sine, K.Power, K.D2, K.Left, K.Left, K.Pi),
      T(@"17_{26}^{35}4", K.D1, K.Subscript, K.D2, K.Right, K.Power, K.D3, K.Right, K.D4,
         K.Left, K.Left, K.D5, K.Left, K.Left, K.Left, K.D6, K.Left, K.Left, K.Left, K.D7),
      T(@"\frac{23}{4}_6^578", K.Fraction, K.D3, K.Right, K.D4, K.Right, K.Power, K.D5, K.Right, K.Subscript,
        K.D6, K.Right, K.Right, K.Right, K.D7, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left,
        K.D2, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.D8),
      T(@"\sqrt[23]{4}_6^578", K.NthRoot, K.D3, K.Right, K.D4, K.Right, K.Power, K.D5, K.Right, K.Subscript,
        K.D6, K.Right, K.Right, K.Right, K.D7, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left,
        K.D2, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.D8),
      T(@"1\frac{\square }{\square }_{\square }^{\square }90", K.Fraction, K.Right, K.Right, K.Power, K.Right,
        K.Subscript, K.Right, K.Right, K.Right, K.D9, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.D1,
        K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.D0),
      T(@"1\sqrt[\square ]{\square }_{\square }^{\square }90", K.NthRoot, K.Right, K.Right, K.Power, K.Right,
        K.Subscript, K.Right, K.Right, K.Right, K.D9, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.D1,
        K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.D0),
      T(@"01\frac{\square }{\square }_{\square }^{\square }90", K.D0, K.Fraction, K.Right, K.Right, K.Power, K.Right,
        K.Subscript, K.Right, K.Right, K.Right, K.D9, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.D1,
        K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.D0),
      T(@"01\sqrt[\square ]{\square }_{\square }^{\square }90", K.D0, K.NthRoot, K.Right, K.Right, K.Power, K.Right,
        K.Subscript, K.Right, K.Right, K.Right, K.D9, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.D1,
        K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.Right, K.D0),
      T(@"1\left[ 2\right] a_3^4", K.BothSquareBrackets, K.Right, K.Power, K.D4, K.Left, K.Left, K.Subscript, K.D3, K.Left, K.Left, K.SmallA, K.Left, K.Left, K.Left, K.Left, K.D1, K.Right, K.D2),
      T(@"1\left[ 2\right] a_3^4", K.BothSquareBrackets, K.Right, K.Power, K.Right, K.Subscript, K.Left, K.Left, K.Left, K.D1, K.Right, K.D2, K.Right, K.SmallA, K.Right, K.D3, K.Right, K.D4),
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
      T(@"", K.VerticalBar, K.VerticalBar, K.Backspace, K.Backspace, K.Backspace)
    ]
    public void Backspace(string latex, params K[] inputs) => Test(latex, inputs);

    [
      Theory,
      T(@"", K.Left, K.Left, K.Backspace, K.Backspace, K.Right, K.Right, K.Backspace, K.Backspace, K.Left),
      T(@"\frac{\square }{3}", K.Slash, K.D3, K.Left, K.Left, K.Backspace, K.Left),
      T(@"1_3", K.D1, K.D2, K.Subscript, K.D3, K.Left, K.Left, K.Backspace),
      T(@"1_3^2", K.D1, K.D4, K.Subscript, K.D3, K.Left, K.Left, K.Power, K.D2, K.Left, K.Left, K.Left, K.Left, K.Backspace),
      T(@"1_2^3", K.D1, K.D4, K.Power, K.D3, K.Left, K.Left, K.Subscript, K.D2, K.Left, K.Left, K.Backspace),
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
      T(@"a\left( c-2\right) ^3", K.SmallA, K.Power, K.D3, K.Left, K.Left, K.BothRoundBrackets, K.SmallC, K.Minus, K.D2),
      T(@"■^{\square }", K.IntegralUpperLimit, K.Left, K.Backspace),
      T(@"■_{\square }", K.IntegralLowerLimit, K.Left, K.Backspace),
      T(@"■_{\square }^{\square }", K.IntegralBothLimits, K.Left, K.Backspace),
      T(@"■^{\square }", K.SummationUpperLimit, K.Left, K.Backspace),
      T(@"■_{\square }", K.SummationLowerLimit, K.Left, K.Backspace),
      T(@"■^{\square }", K.ProductUpperLimit, K.Left, K.Backspace),
      T(@"■_{\square }", K.ProductLowerLimit, K.Left, K.Backspace),
    ]
    public void LeftRightBackspace(string latex, params K[] inputs) => Test(latex, inputs);

    [Theory, T(@"\square ^■", K.Power), T(@"\square _■", K.Subscript)]
    public void ScriptsAtBeginningOfLine(string latex, params K[] inputs) => Test(latex, inputs);

    [
      Theory,
      T(@"\pi _{\square }", K.Subscript, K.Left, K.SmallPi),
      T(@"\pi \theta _{\square }", K.SmallPi, K.Subscript, K.Left, K.SmallTheta),
      T(@"eA^{\square }", K.BaseEPower, K.Left, K.A),
      T(@"e\frac{■}{\square }^{\square }", K.BaseEPower, K.Left, K.Fraction),
      T(@"e\sqrt[3]{■}^{\square }", K.BaseEPower, K.Left, K.CubeRoot),
      T(@"eAB^{\square }", K.BaseEPower, K.Left, K.A, K.B),
      T(@"eA\frac{\square }{\square }\sqrt{\square }B^{\square }",
        K.BaseEPower, K.Left, K.A, K.Fraction, K.Right, K.Right, K.SquareRoot, K.Right, K.B),
      T(@"eA\frac{\square }{\square }\sqrt[3]{\square }B_{\square }",
        K.SmallE, K.Subscript, K.Left, K.A, K.Fraction, K.Right, K.Right, K.CubeRoot, K.Right, K.B),
      T(@"eA\frac{\square }{\square }\sqrt[\square ]{\square }B_{\square }^{\square }",
        K.BaseEPower, K.Left, K.Subscript, K.Left, K.A, K.Fraction, K.Right, K.Right, K.NthRoot, K.Right, K.Right, K.B),
      T(@"\int 2^{\square }", K.IntegralUpperLimit, K.Left, K.D2),
      T(@"\int 2_{\square }", K.IntegralLowerLimit, K.Left, K.D2),
      T(@"\int \log _■^{\square }", K.IntegralUpperLimit, K.Left, K.LogarithmWithBase),
      T(@"\sum \prod _{\square }^■", K.SummationLowerLimit, K.Left, K.ProductUpperLimit),
      T(@"\log \log _■", K.LogarithmWithBase, K.Left, K.LogarithmWithBase),
      T(@"\lim \lim _■", K.LimitWithBase, K.Left, K.LimitWithBase),
      T(@"\log \lim _■", K.LogarithmWithBase, K.Left, K.LimitWithBase),
      T(@"\prod \int ^■", K.ProductUpperLimit, K.Left, K.IntegralUpperLimit),
      T(@"\int \prod _■^{\square }", K.IntegralBothLimits, K.Left, K.ProductBothLimits),
      T(@"\sum \int _■^{\square }", K.SummationBothLimits, K.Left, K.IntegralLowerLimit),
      T(@"\prod \prod _■^{\square }", K.ProductBothLimits, K.Left, K.ProductBothLimits),
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
      T(@"\prod _{i=1}^{\infty }", K.A, K.SummationBothLimits, K.SmallI, K.Equals, K.D1, K.Right, K.Infinity,
        K.Left, K.Left, K.Left, K.Left, K.Left, K.Left, K.Backspace, K.Backspace, K.Product),
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
      T(@"\frac{1}{2}\times \frac{1}{■}", K.Slash, K.D2, K.Right, K.D1, K.Slash),
      T(@"\frac{1}{2}\times \frac{2}{■}", K.Slash, K.D2, K.Right, K.D2, K.Slash),

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

      T(@"\frac{\left( \square \right) }{■}", K.BothRoundBrackets, K.Right, K.Slash),
      T(@"\frac{\left[ \square \right] }{■}", K.BothSquareBrackets, K.Right, K.Slash),
      T(@"\frac{\left\{ \square \right\} }{■}", K.BothCurlyBrackets, K.Right, K.Slash),
      T(@"\frac{\left| \square \right| }{■}", K.Absolute, K.Right, K.Slash),
      T(@"\frac{\left( \square \right) \left[ \square \right] }{■}", K.BothRoundBrackets, K.Right, K.BothSquareBrackets, K.Right, K.Slash),
      T(@"+\frac{\left( \square \right) \left[ \square \right] }{■}", K.Plus, K.BothRoundBrackets, K.Right, K.BothSquareBrackets, K.Right, K.Slash),
      T(@"(\frac{()}{■}", K.LeftRoundBracket, K.LeftRoundBracket, K.RightRoundBracket, K.Right, K.Slash),
      T(@"(\frac{\left( \square \right) }{■}", K.LeftRoundBracket, K.BothRoundBrackets, K.Right, K.Slash),
      T(@"\left( \frac{1}{■}\right) ", K.BothRoundBrackets, K.Slash),
      T(@"\frac{(\frac{1}{\square })}{■}", K.LeftRoundBracket, K.Slash, K.Right, K.RightRoundBracket, K.Slash),
      T(@"\frac{\left( \frac{1}{\square }\right) }{■}", K.BothRoundBrackets, K.Slash, K.Right, K.Right, K.Slash),
      T(@"(\frac{[\} }{■}", K.LeftRoundBracket, K.LeftSquareBracket, K.RightCurlyBracket, K.Slash),
      T(@"\{ \frac{[0,\infty )}{■}",
        K.LeftCurlyBracket, K.LeftSquareBracket, K.D0, K.Comma, K.Infinity, K.RightRoundBracket, K.Slash),
      T(@"\frac{(\{ \} )([])}{■}",
        K.LeftRoundBracket, K.LeftCurlyBracket, K.RightCurlyBracket, K.RightRoundBracket,
        K.LeftRoundBracket, K.LeftSquareBracket, K.RightSquareBracket, K.RightRoundBracket, K.Slash),
      T(@"(\frac{(\{ \} )([])}{■}", K.LeftRoundBracket,
        K.LeftRoundBracket, K.LeftCurlyBracket, K.RightCurlyBracket, K.RightRoundBracket,
        K.LeftRoundBracket, K.LeftSquareBracket, K.RightSquareBracket, K.RightRoundBracket, K.Slash),

      T(@"\frac{(1+2)}{■}", K.LeftRoundBracket, K.D1, K.Plus, K.D2, K.RightRoundBracket, K.Slash),
      T(@"\frac{\left( 1+2\right) }{■}", K.BothRoundBrackets, K.D1, K.Plus, K.D2, K.Right, K.Slash),
      T(@"|1+\frac{2|}{■}", K.VerticalBar, K.D1, K.Plus, K.D2, K.VerticalBar, K.Slash),
      T(@"\frac{\left| 1+2\right| }{■}", K.Absolute, K.D1, K.Plus, K.D2, K.Right, K.Slash),
      T(@"1+\frac{2}{■}", K.D1, K.Plus, K.D2, K.Slash),
      T(@"1-\frac{2}{■}", K.D1, K.Minus, K.D2, K.Slash),
      T(@"1\times \frac{2}{■}", K.D1, K.Multiply, K.D2, K.Slash),
      T(@"1\div \frac{2}{■}", K.D1, K.Divide, K.D2, K.Slash),
      T(@"1:\frac{2}{■}", K.D1, K.Ratio, K.D2, K.Slash),
      T(@"1=\frac{2}{■}", K.D1, K.Equals, K.D2, K.Slash),
      T(@"1\neq \frac{2}{■}", K.D1, K.NotEquals, K.D2, K.Slash),
      T(@"1<\frac{2}{■}", K.D1, K.LessThan, K.D2, K.Slash),
      T(@"1\leq \frac{2}{■}", K.D1, K.LessOrEquals, K.D2, K.Slash),
      T(@"1>\frac{2}{■}", K.D1, K.GreaterThan, K.D2, K.Slash),
      T(@"1\geq \frac{2}{■}", K.D1, K.GreaterOrEquals, K.D2, K.Slash),
      T(@"\frac{1}{\frac{2}{■}}", K.D1, K.Slash, K.D2, K.Slash),
      T(@"\sqrt{x+\frac{2}{■}}", K.SquareRoot, K.SmallX, K.Plus, K.D2, K.Slash),
      T(@"\frac{\left( x+\sqrt{2}\right) }{■}", K.BothRoundBrackets, K.SmallX, K.Plus, K.SquareRoot, K.D2, K.Right, K.Right, K.Slash),
      T(@"\frac{(x+\sqrt{2})}{■}", K.LeftRoundBracket, K.SmallX, K.Plus, K.SquareRoot, K.D2, K.Right, K.RightRoundBracket, K.Slash),
      T(@"\sqrt[X2Z+\frac{X2Z}{■}]{\square }", K.NthRoot, K.X, K.D2, K.Z, K.Plus, K.X, K.D2, K.Z, K.Slash),

      T(@"\frac{\int }{■}", K.Integral, K.Slash),
      T(@"\frac{1\int }{■}", K.D1, K.Integral, K.Slash),
      T(@"\frac{\int 1}{■}", K.Integral, K.D1, K.Slash),
      T(@"+\frac{\prod }{■}", K.Plus, K.Product, K.Slash),
      T(@"\frac{x}{2}\times \frac{\sum }{■}", K.SmallX, K.Slash, K.D2, K.Right, K.Summation, K.Slash),
      T(@"\frac{\lim _{x\rightarrow 2}}{■}", K.LimitWithBase, K.SmallX, K.RightArrow, K.D2, K.Right, K.Slash),
    ]
    public void Slash(string latex, params K[] inputs) => Test(latex, inputs);

    [Theory,
      T(@"", K.Up, K.Up, K.Up, K.Down, K.Down, K.Down),
      T(@"1", K.Up, K.D1), T(@"1", K.Down, K.D1),
      T(@"\frac{\square }{■}", K.Fraction, K.Down),
      T(@"\frac{\square }{1}", K.Fraction, K.Down, K.D1),
      T(@"\frac{■}{\square }", K.Fraction, K.Down, K.Up),
      T(@"\frac{1}{\square }", K.Fraction, K.Down, K.Up, K.D1),
      T(@"\frac{\square }{■}", K.Fraction, K.Down, K.Up, K.Down),
      T(@"\frac{\square }{1}", K.Fraction, K.Down, K.Up, K.Down, K.D1),
      T(@"\frac{123x456}{y}", K.Fraction, K.D1, K.D2, K.D3, K.D4, K.D5, K.D6, K.Down, K.Up, K.SmallX, K.Down, K.SmallY),
      T(@"\frac{y}{123x456}", K.Fraction, K.Down, K.D1, K.D2, K.D3, K.D4, K.D5, K.D6, K.Up, K.Down, K.SmallX, K.Up, K.SmallY),
      T(@"\frac{1234z56}{xy}", K.Fraction, K.D1, K.D2, K.D3, K.D4, K.D5, K.D6, K.Down, K.SmallX, K.SmallY, K.Up, K.SmallZ),
      T(@"\frac{xy}{1234z56}", K.Fraction, K.Down, K.D1, K.D2, K.D3, K.D4, K.D5, K.D6, K.Up, K.SmallX, K.SmallY, K.Down, K.SmallZ),
      T(@"\frac{\sqrt{2}}{■}", K.Fraction, K.SquareRoot, K.D2, K.Down),
      T(@"\frac{■}{\sqrt{2}}", K.Fraction, K.Down, K.SquareRoot, K.D2, K.Up),
      T(@"\frac{\sqrt{789}}{\square }", K.Fraction, K.SquareRoot, K.D8, K.D9, K.Down, K.Up, K.D7),
      T(@"\frac{\square }{\sqrt{789}}", K.Fraction, K.Down, K.SquareRoot, K.D8, K.D9, K.Up, K.Down, K.D7),
      T(@"\frac{\left( a\right) }{\left( b\right) }", K.Fraction, K.BothRoundBrackets, K.Down, K.BothRoundBrackets, K.Up, K.SmallA, K.Down, K.SmallB),
      T(@"\frac{\left( b\right) }{\left( a\right) }", K.Fraction, K.Down, K.BothRoundBrackets, K.Up, K.BothRoundBrackets, K.Down, K.SmallA, K.Up, K.SmallB),
      T(@"2^{ab}c", K.D2, K.Power, K.SmallA, K.Up, K.SmallB, K.Down, K.SmallC),
      T(@"2_{ab}c", K.D2, K.Subscript, K.SmallA, K.Down, K.SmallB, K.Up, K.SmallC),
      T(@"\square _1^3", K.Subscript, K.Up, K.Power, K.Down, K.D1, K.Up, K.D3),
      T(@"123^■", K.D1, K.Power, K.Down, K.D2, K.D3, K.Up),
      T(@"123_■", K.D1, K.Subscript, K.Up, K.D2, K.D3, K.Down),
      T(@"1^{\square ^{\square ^{4^{\square }}}}", K.Power, K.Power, K.Power, K.Power, K.Down, K.Down, K.Down, K.Down, K.D1, K.Up, K.Up, K.Up, K.D4),
      T(@"1_{\square _{\square _{4_{\square }}}}", K.Subscript, K.Subscript, K.Subscript, K.Subscript, K.Up, K.Up, K.Up, K.Up, K.D1, K.Down, K.Down, K.Down, K.D4),
      T(@"2_{cab}^{\square }", K.D2, K.Subscript, K.SmallA, K.SmallB, K.Up, K.Power, K.Down, K.SmallC),
      T(@"2c_{\square }^{ab}", K.D2, K.Power, K.SmallA, K.SmallB, K.Down, K.Subscript, K.Up, K.SmallC),
      T(@"\square ^{\square }2", K.Power, K.Down, K.D2),
      T(@"\square _{\square }2", K.Subscript, K.Up, K.D2),
      T(@"1^{\square ^{\square ^{\square ^5}}}", K.Power, K.Power, K.Power, K.Power, K.Down, K.Down, K.Down, K.Down, K.D1, K.Up, K.Up, K.Up, K.Up, K.D5),
      T(@"1_{\square _{\square _{\square _5}}}", K.Subscript, K.Subscript, K.Subscript, K.Subscript, K.Up, K.Up, K.Up, K.Up, K.D1, K.Down, K.Down, K.Down, K.Down, K.D5),
      T(@"\frac{1^{\square ^{\square ^{4^{\square }}}}}{\square }", K.Fraction, K.Power, K.Power, K.Power, K.Power, K.Down, K.Down, K.Down, K.Down, K.D1, K.Up, K.Up, K.Up, K.D4),
      T(@"\frac{1_{\square _{\square _{4_{\square }}}}}{\square }", K.Fraction, K.Subscript, K.Subscript, K.Subscript, K.Subscript, K.Up, K.Up, K.Up, K.Up, K.D1, K.Down, K.Down, K.Down, K.D4),
      T(@"\frac{\square }{1^{\square ^{\square ^{4^{\square }}}}}", K.Fraction, K.Down, K.Power, K.Power, K.Power, K.Power, K.Down, K.Down, K.Down, K.Down, K.D1, K.Up, K.Up, K.Up, K.D4),
      T(@"\frac{\square }{1_{\square _{\square _{4_{\square }}}}}", K.Fraction, K.Down, K.Subscript, K.Subscript, K.Subscript, K.Subscript, K.Up, K.Up, K.Up, K.Up, K.D1, K.Down, K.Down, K.Down, K.D4),
      T(@"\frac{1^{\square ^{\square ^{\square ^5}}}}{\square }", K.Fraction, K.Power, K.Power, K.Power, K.Power, K.Down, K.Down, K.Down, K.Down, K.D1, K.Up, K.Up, K.Up, K.Up, K.D5),
      T(@"\frac{1_{\square _{\square _{\square _5}}}}{\square }", K.Fraction, K.Subscript, K.Subscript, K.Subscript, K.Subscript, K.Up, K.Up, K.Up, K.Up, K.D1, K.Down, K.Down, K.Down, K.Down, K.D5),
      T(@"\frac{\square }{1^{\square ^{\square ^{\square ^5}}}}", K.Fraction, K.Down, K.Power, K.Power, K.Power, K.Power, K.Down, K.Down, K.Down, K.Down, K.D1, K.Up, K.Up, K.Up, K.Up, K.D5),
      T(@"\frac{\square }{1_{\square _{\square _{\square _5}}}}", K.Fraction, K.Down, K.Subscript, K.Subscript, K.Subscript, K.Subscript, K.Up, K.Up, K.Up, K.Up, K.D1, K.Down, K.Down, K.Down, K.Down, K.D5)]
    public void UpDownNavigation(string latex, params K[] inputs) => Test(latex, new TestFont(10), inputs);

    [Theory,
      T(@"\square ^21", K.Power, K.D2, K.Left, K.Down, K.D1),
      T(@"\square _21", K.Subscript, K.D2, K.Left, K.Up, K.D1),
      T(@"01^2", K.D0, K.Power, K.D2, K.Left, K.Down, K.D1),
      T(@"01_2", K.D0, K.Subscript, K.D2, K.Left, K.Up, K.D1),
      T(@"1^{23}", K.Power, K.D2, K.D3, K.Left, K.Left, K.Down, K.D1),
      T(@"1_{23}", K.Subscript, K.D2, K.D3, K.Left, K.Left, K.Up, K.D1),
      T(@"x^{zy}", K.SmallX, K.Power, K.SmallY, K.Left, K.Left, K.Up, K.SmallZ),
      T(@"x_{zy}", K.SmallX, K.Subscript, K.SmallY, K.Left, K.Left, K.Down, K.SmallZ),
      T(@"1^23", K.D1, K.Power, K.Right, K.D3, K.Left, K.Left, K.Up, K.D2),
      T(@"1_23", K.D1, K.Subscript, K.Right, K.D3, K.Left, K.Left, K.Down, K.D2),
      T(@"1^{\left( 2\right) }3", K.D1, K.Power, K.BothRoundBrackets, K.Right, K.Right, K.D3, K.Left, K.Up, K.Left, K.D2),
      T(@"1_{\left( 2\right) }3", K.D1, K.Subscript, K.BothRoundBrackets, K.Right, K.Right, K.D3, K.Left, K.Down, K.Left, K.D2),
      T(@"\frac{a_{123x456}}{b^{6543y21}}", K.Fraction, K.SmallA, K.Subscript, K.D1, K.D2, K.D3, K.D4, K.D5, K.D6, K.Down, K.Down,
        K.SmallB, K.Power, K.D6, K.D5, K.D4, K.D3, K.D2, K.D1, K.Left, K.Left, K.Left, K.Up, K.SmallX, K.Down, K.SmallY),
      T(@"\frac{a^{1e23456}b}{cd_{654321}}", K.Fraction, K.SmallA, K.Power, K.D1, K.D2, K.D3, K.D4, K.D5, K.D6, K.Down, K.SmallB, K.Down,
        K.SmallC, K.Subscript, K.D6, K.D5, K.D4, K.D3, K.D2, K.D1, K.Left, K.Left, K.Left, K.Up, K.SmallD, K.Up, K.SmallE),
      T(@"1^{\square ^{\square ^{4^{\square }}}}", K.Power, K.Power, K.Power, K.Power, K.Left, K.Left, K.Left, K.Left, K.D1, K.Up, K.Up, K.Up, K.D4),
      T(@"1_{\square _{\square _{4_{\square }}}}", K.Subscript, K.Subscript, K.Subscript, K.Subscript, K.Left, K.Left, K.Left, K.Left, K.D1, K.Down, K.Down, K.Down, K.D4),
      T(@"\frac{1^{\square ^{\square ^{4^{\square }}}}}{\square }", K.Fraction, K.Power, K.Power, K.Power, K.Power, K.Left, K.Left, K.Left, K.Left, K.D1, K.Up, K.Up, K.Up, K.D4),
      T(@"\frac{1_{\square _{\square _{4_{\square }}}}}{\square }", K.Fraction, K.Subscript, K.Subscript, K.Subscript, K.Subscript, K.Left, K.Left, K.Left, K.Left, K.D1, K.Down, K.Down, K.Down, K.D4),
      T(@"\frac{\square }{1^{\square ^{\square ^{4^{\square }}}}}", K.Fraction, K.Down, K.Power, K.Power, K.Power, K.Power, K.Left, K.Left, K.Left, K.Left, K.D1, K.Up, K.Up, K.Up, K.D4),
      T(@"\frac{\square }{1_{\square _{\square _{4_{\square }}}}}", K.Fraction, K.Down, K.Subscript, K.Subscript, K.Subscript, K.Subscript, K.Left, K.Left, K.Left, K.Left, K.D1, K.Down, K.Down, K.Down, K.D4),
      T(@"1^{\square ^{\square ^{\square ^{\square }}7}8}9", K.D1, K.Power, K.Power, K.Power, K.Power, K.Right, K.Down, K.Down, K.Down, K.D9, K.Left, K.Up, K.Up, K.D7, K.Down, K.D8),
      T(@"1_{\square _{\square _{\square _{\square }}7}8}9", K.D1, K.Subscript, K.Subscript, K.Subscript, K.Subscript, K.Right, K.Up, K.Up, K.Up, K.D9, K.Left, K.Down, K.Down, K.D7, K.Up, K.D8),
      T(@"\frac{1^{\square ^{\square ^{\square ^{\square }}7}8}9}{\square }", K.Fraction, K.D1, K.Power, K.Power, K.Power, K.Power, K.Right, K.Down, K.Down, K.Down, K.D9, K.Left, K.Up, K.Up, K.D7, K.Down, K.D8),
      T(@"\frac{1_{\square _{\square _{\square _{\square }}7}8}9}{\square }", K.Fraction, K.D1, K.Subscript, K.Subscript, K.Subscript, K.Subscript, K.Right, K.Up, K.Up, K.Up, K.D9, K.Left, K.Down, K.Down, K.D7, K.Up, K.D8),
      T(@"\frac{\square }{1^{\square ^{\square ^{\square ^{\square }}7}8}9}", K.Fraction, K.Down, K.D1, K.Power, K.Power, K.Power, K.Power, K.Right, K.Down, K.Down, K.Down, K.D9, K.Left, K.Up, K.Up, K.D7, K.Down, K.D8),
      T(@"\frac{\square }{1_{\square _{\square _{\square _{\square }}7}8}9}", K.Fraction, K.Down, K.D1, K.Subscript, K.Subscript, K.Subscript, K.Subscript, K.Right, K.Up, K.Up, K.Up, K.D9, K.Left, K.Down, K.Down, K.D7, K.Up, K.D8),
      T(@"\frac{ef^{gh}}{\square }", K.Fraction, K.SmallE, K.Power, K.SmallH, K.Left, K.Down, K.SmallF, K.Up, K.SmallG),
      T(@"\frac{ef_{gh}}{\square }", K.Fraction, K.SmallE, K.Subscript, K.SmallH, K.Left, K.Up, K.SmallF, K.Down, K.SmallG),
      T(@"\frac{\square }{ef^{gh}}", K.Fraction, K.Down, K.SmallE, K.Power, K.SmallH, K.Left, K.Down, K.SmallF, K.Up, K.SmallG),
      T(@"\frac{\square }{ef_{gh}}", K.Fraction, K.Down, K.SmallE, K.Subscript, K.SmallH, K.Left, K.Up, K.SmallF, K.Down, K.SmallG),
      T(@"\frac{a^b}{c}", K.Fraction, K.SmallA, K.Power, K.SmallB, K.Left, K.Down, K.Down, K.SmallC),
      T(@"\frac{a_{tb}}{c}", K.Fraction, K.SmallA, K.Subscript, K.SmallB, K.Left, K.Up, K.Down, K.SmallT, K.Down, K.SmallC),
      T(@"\frac{c}{a^{tb}}", K.Fraction, K.Down, K.SmallA, K.Power, K.SmallB, K.Left, K.Down, K.Up, K.SmallT, K.Up, K.SmallC),
      T(@"\frac{c}{a_b}", K.Fraction, K.Down, K.SmallA, K.Subscript, K.SmallB, K.Left, K.Up, K.Up, K.SmallC)]
    public void FourDirectionalNavigation(string latex, params K[] inputs) => Test(latex, new TestFont(10), inputs);

    [Fact]
    public void NavigationUsesNonzeroWidthGeometryAndStaysInBranch() =>
      Test(@"\frac{a^b}{c_d}", new TestFont(10), K.Fraction, K.SmallA, K.Power, K.SmallB,
        K.Left, K.Down, K.Down, K.SmallC, K.Subscript, K.SmallD);

    [Fact]
    public void NavigationSupportsZeroWidthFont() =>
      Test(@"\frac{\square }{1}", K.Fraction, K.Down, K.D1);

    [Fact]
    public void AssigningInsertionIndexClearsVerticalNavigationState() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(context, new TestFont(10));
      keyboard.KeyPress(K.Power, K.Down);
      keyboard.InsertionIndex = new(0);
      keyboard.KeyPress(K.D1, K.Left);
      Assert.Equal("1^■", keyboard.LaTeX);
    }

    [Fact]
    public void TableIndexRetainsRowAndColumnAndMovesVertically() {
      var table = new CSharpMath.Atom.Atoms.Table();
      table.SetCell(new CSharpMath.Atom.MathList(new CSharpMath.Atom.Atoms.Number("12")), 0, 0);
      table.SetCell(new CSharpMath.Atom.MathList(new CSharpMath.Atom.Atoms.Number("3")), 1, 0);
      var keyboard = new MathKeyboard<TestFont, TGlyph>(context, new TestFont(10));
      keyboard.MathList.Add(table);
      var atom = new MathListIndex(0).TableCell(0, 0, new MathListIndex(0));
      var first = new MathListIndex(0).TableCell(0, 0, new MathListIndex(1));
      Assert.IsType<CSharpMath.Atom.Atoms.Number>(keyboard.MathList.AtomAt(atom));
      keyboard.InsertionIndex = first;
      keyboard.KeyPress(K.Down);
      Assert.Equal(new MathListIndex(0).TableCell(1, 0, new MathListIndex(1)), keyboard.InsertionIndex);
      keyboard.KeyPress(K.Up);
      Assert.Equal(first, keyboard.InsertionIndex);
    }

    [Fact]
    public void TableNavigationSkipsMissingRowsAndStopsAtBoundaries() {
      var table = new CSharpMath.Atom.Atoms.Table();
      table.SetCell(new CSharpMath.Atom.MathList(new CSharpMath.Atom.Atoms.Number("1")), 0, 0);
      table.SetCell(new CSharpMath.Atom.MathList(new CSharpMath.Atom.Atoms.Number("2")), 2, 0);
      var keyboard = new MathKeyboard<TestFont, TGlyph>(context, new TestFont(10));
      keyboard.MathList.Add(table);
      var first = new MathListIndex(0).TableCell(0, 0, new MathListIndex(0));
      keyboard.InsertionIndex = first;
      keyboard.KeyPress(K.Up);
      Assert.Equal(first, keyboard.InsertionIndex);
      keyboard.KeyPress(K.Down);
      var last = new MathListIndex(0).TableCell(2, 0, new MathListIndex(0));
      Assert.Equal(last, keyboard.InsertionIndex);
      keyboard.KeyPress(K.Up);
      Assert.Equal(first, keyboard.InsertionIndex);
    }

    [Fact]
    public void TableNavigationComposesWithFractionPathAndSkipsEmptyRows() {
      var table = new CSharpMath.Atom.Atoms.Table();
      table.SetCell(new CSharpMath.Atom.MathList(new CSharpMath.Atom.Atoms.Number("1")), 0, 0);
      table.SetCell(new CSharpMath.Atom.MathList(new CSharpMath.Atom.Atoms.Number("2")), 2, 0);
      var fraction = new CSharpMath.Atom.Atoms.Fraction(
        new CSharpMath.Atom.MathList(table), new CSharpMath.Atom.MathList());
      var keyboard = new MathKeyboard<TestFont, TGlyph>(context, new TestFont(10));
      keyboard.MathList.Add(fraction);
      var tableIndex = new MathListIndex(0).TableCell(0, 0, new MathListIndex(0));
      var nestedIndex = tableIndex.Wrap(0, MathListSubIndexType.Numerator);
      Assert.IsType<CSharpMath.Atom.Atoms.Number>(keyboard.MathList.AtomAt(nestedIndex));
      keyboard.InsertionIndex = nestedIndex;
      keyboard.KeyPress(K.Down);
      Assert.Equal(new MathListIndex(0).TableCell(2, 0, new MathListIndex(0)).Wrap(0, MathListSubIndexType.Numerator), keyboard.InsertionIndex);
    }

  }
}
