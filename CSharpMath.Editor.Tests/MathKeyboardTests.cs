using CSharpMath.FrontEnd;
using CSharpMath.Tests.FrontEnd;
using Xunit;
using T = Xunit.InlineDataAttribute; // 'T'est
using K = CSharpMath.Editor.MathKeyboardInput; // 'K'ey

namespace CSharpMath.Editor.Tests {
  public class MathKeyboardTests {
    private static readonly TypesettingContext<TestFont, char> context = TestTypesettingContexts.Instance;
    [
      Theory,
      T(""),
      T("1", K.D1),
      T("x", K.SmallX),
      T("X_{2_3}", K.X, K.Subscript, K.D2, K.Subscript, K.D3),
      T(@"+-\times \div ", K.Divide, K.Left, K.Multiply, K.Left, K.Minus, K.Left, K.Plus),
      T(@"\sin \cos \tan \arcsin \arccos \arctan ", K.ArcSine, K.ArcCosine, K.Left, K.Left,
        K.Sine, K.Cosine, K.Right, K.Right, K.ArcTangent, K.Left, K.Left, K.Left, K.Tangent),
      T(@"x^{\frac{2}{■}}", K.SmallX, K.Power, K.D2, K.Slash),
      // https://github.com/verybadcat/CSharpMath/issues/39
      T(@"x^{\frac{123}{■}}", K.SmallX, K.Power, K.D1, K.D2, K.D3, K.Slash),
      T(@"\frac{1}{■}", K.Slash),
       // https://github.com/kostub/MathEditor/issues/18
      T(@"\frac{4}{\frac{4}{■}}", K.D4, K.Slash, K.D4, K.Slash),
      T(@"□^{□^{□^■}}", K.Power, K.Power, K.Power),
      T(@"e^{\square }", K.Power, K.Left, K.SmallE, K.Right),
      T(@"e^■", K.Power, K.Left, K.SmallE, K.Left),
      T(@"e^■", K.BaseEPower)
    ]
    public void KeyPressTests(string latex, params K[] inputs) {
      var keyboard = new MathKeyboard<TestFont, char>(context);
      keyboard.KeyPress(inputs);
      Assert.Equal(latex, keyboard.LaTeX);
    }
  }
}