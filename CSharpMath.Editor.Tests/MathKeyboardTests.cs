using CSharpMath.Editor;
using CSharpMath.FrontEnd;
using CSharpMath.Tests.FrontEnd;
using Xunit;

namespace CSharpMath.Editor.Tests {
  public class MathKeyboardTests {

    public const float FontSize = 20;
    public static readonly TestFont Font = new TestFont(FontSize);
    private static readonly TypesettingContext<TestFont, char> context = TestTypesettingContexts.Instance;

    [Fact]
    // https://github.com/verybadcat/CSharpMath/issues/39
    public void SlashInsideOfPowerTest() {
      var keyboard = new MathKeyboard<TestFont, char>(context);
      keyboard.KeyPress(MathKeyboardInput.SmallX);
      keyboard.KeyPress(MathKeyboardInput.Power);
      keyboard.KeyPress(MathKeyboardInput.D2);
      keyboard.KeyPress(MathKeyboardInput.Slash);
      Assert.Equal("x^{\\frac{2}{■}}", keyboard.LaTeX);
    }

    [Fact]
    public void SlashInsideOfPowerTest2() {
      var keyboard = new MathKeyboard<TestFont, char>(context);
      keyboard.KeyPress(MathKeyboardInput.SmallX);
      keyboard.KeyPress(MathKeyboardInput.Power);
      keyboard.KeyPress(MathKeyboardInput.D1);
      keyboard.KeyPress(MathKeyboardInput.D2);
      keyboard.KeyPress(MathKeyboardInput.D3);
      keyboard.KeyPress(MathKeyboardInput.Slash);
      Assert.Equal("x^{\\frac{123}{■}}", keyboard.LaTeX);
    }

    [Fact]
    public void DefaultNumeratorTest() {
      var keyboard = new MathKeyboard<TestFont, char>(context);
      keyboard.KeyPress(MathKeyboardInput.Slash);
      Assert.Equal("\\frac{1}{■}", keyboard.LaTeX);
    }

    [Fact]
    // https://github.com/kostub/MathEditor/issues/18
    public void SlashAfterSlashTest() {
      var keyboard = new MathKeyboard<TestFont, char>(context);
      keyboard.KeyPress(MathKeyboardInput.D4);
      keyboard.KeyPress(MathKeyboardInput.Slash);
      keyboard.KeyPress(MathKeyboardInput.D4);
      keyboard.KeyPress(MathKeyboardInput.Slash);
      Assert.Equal("\\frac{4}{\\frac{4}{■}}", keyboard.LaTeX);
    }
  }
}