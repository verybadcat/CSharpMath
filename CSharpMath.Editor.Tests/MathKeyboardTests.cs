using CSharpMath.FrontEnd;
using CSharpMath.Tests.FrontEnd;
using Xunit;
using T = Xunit.InlineDataAttribute; // 'T'est
using K = CSharpMath.Editor.MathKeyboardInput; // 'K'ey

namespace CSharpMath.Editor.Tests {
  public class MathKeyboardTests {
    private static readonly TypesettingContext<TestFont, char> context = TestTypesettingContexts.Instance;
    static void Test(string latex, K[] inputs) {
      var keyboard = new MathKeyboard<TestFont, char>(context);
      keyboard.KeyPress(inputs);
      Assert.Equal(latex, keyboard.LaTeX);
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
      T(@"X_{2_3}", K.X, K.Subscript, K.D2, K.Subscript, K.D3),
      T(@"x^{\frac{2}{■}}", K.SmallX, K.Power, K.D2, K.Slash),
      // https://github.com/verybadcat/CSharpMath/issues/39
      T(@"x^{\frac{123}{■}}", K.SmallX, K.Power, K.D1, K.D2, K.D3, K.Slash),
      T(@"\frac{1}{■}", K.Slash),
      // https://github.com/kostub/MathEditor/issues/18
      T(@"\frac{4}{\frac{4}{■}}", K.D4, K.Slash, K.D4, K.Slash),
      T(@"□^{□^{□^■}}", K.Power, K.Power, K.Power),
      T(@"e^■", K.SmallE, K.Power),
      T(@"e^■", K.BaseEPower),
      T(@"\sqrt{3}", K.SquareRoot, K.D3),
      T(@"\sqrt[3]{3}", K.CubeRoot, K.D3),
    ]
    public void AtomInput(string latex, params K[] inputs) => Test(latex, inputs);

    [
      Theory,
      T(@"", K.Left, K.Left, K.Left, K.Right, K.Right, K.Right),
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
      T(@"\sin Π^2", K.Sine, K.Power, K.D2, K.Left, K.Left, K.Pi),
    ]
    public void LeftRightNavigation(string latex, params K[] inputs) => Test(latex, inputs);

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
    ]
    public void LeftRightBackspace(string latex, params K[] inputs) => Test(latex, inputs);
  }
}