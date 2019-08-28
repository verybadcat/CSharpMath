using CSharpMath.Editor;
using CSharpMath.FrontEnd;
using CSharpMath.Tests.FrontEnd;
using Xunit;

namespace CSharpMath.Editor.Tests {
  public class MathKeyboardTests {

    private static readonly TypesettingContext<TestFont, char> context = TestTypesettingContexts.Instance;

    [Theory]
    [InlineData("x^{\\frac{2}{■}}", new[] {
        MathKeyboardInput.SmallX,
        MathKeyboardInput.Power,
        MathKeyboardInput.D2,
        MathKeyboardInput.Slash })]
    // https://github.com/verybadcat/CSharpMath/issues/39
    [InlineData("x^{\\frac{123}{■}}", new[] {
        MathKeyboardInput.SmallX,
        MathKeyboardInput.Power,
        MathKeyboardInput.D1,
        MathKeyboardInput.D2,
        MathKeyboardInput.D3,
        MathKeyboardInput.Slash })]
    [InlineData("\\frac{1}{■}", new[] { MathKeyboardInput.Slash })]
    // https://github.com/kostub/MathEditor/issues/18
    [InlineData("\\frac{4}{\\frac{4}{■}}", new[] {
        MathKeyboardInput.D4,
        MathKeyboardInput.Slash,
        MathKeyboardInput.D4,
        MathKeyboardInput.Slash })]
    public void KeyPressTests(string latex, MathKeyboardInput[] inputs) {
      var keyboard = new MathKeyboard<TestFont, char>(context);
      keyboard.KeyPress(inputs);
      Assert.Equal(latex, keyboard.LaTeX);
    }
  }
}