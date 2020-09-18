using System;
using System.Collections.Generic;
using System.Linq;
using CSharpMath.Editor;
using Xamarin.Forms;
using Xunit;
namespace CSharpMath.Forms.Tests {
  public class ButtonTests {
    [Fact]
    public void ChangingButtonTextColorDoesNotChangeLatexPropertyValue() {
      var latexContent = @"1\leq 2";
      var newColor = Color.Gray;

      var mathButton = new MathButton { Content = new MathView { LaTeX = latexContent } };
      mathButton.TextColor = newColor;
      Assert.Equal(mathButton.TextColor, newColor);
      Assert.Equal(latexContent, mathButton.Content.LaTeX);

      var textButton = new TextButton { Content = new TextView { LaTeX = latexContent } };
      textButton.TextColor = newColor;
      Assert.Equal(mathButton.TextColor, newColor);
      Assert.Equal(latexContent, textButton.Content.LaTeX);
    }
    [Theory]
    [MemberData(nameof(TheMathKeyboardInputs))]
    public void AllMathInputButtonsHaveLatexContent(MathKeyboardInput mathKeyboardInput) {
      var mathInputButton = new MathInputButton { Input = mathKeyboardInput };
      Assert.False(string.IsNullOrEmpty(mathInputButton.Content?.LaTeX));
    }
    [Theory]
    [MemberData(nameof(TheMathKeyboardInputs))]
    public void MathInputButtonsHaveBlackTextColorByDefault(MathKeyboardInput mathKeyboardInput) {
      var mathInputButton = new MathInputButton { Input = mathKeyboardInput };
      // At the time of writing this test, phatom = @"{\color{#00FFFFFF}{|}}" (Xamarin.Forms.Color.Transparent = "#00FFFFFF") is used as there is no \phantom command yet.
      // As soon as \phantom has been implemented, the call .Replace(LatexHelper.phantom, "") can be removed.
      Assert.DoesNotContain(@"\color", mathInputButton.Content.NotNull().LaTeX.NotNull().Replace(LatexHelper.phantom, ""));
      Assert.Equal(Color.Black, mathInputButton.TextColor);
    }
    [Theory]
    [MemberData(nameof(TheMathKeyboardInputs))]
    public void MathInputButtonsHaveTransparentBackgroundByDefault(MathKeyboardInput mathKeyboardInput) {
      var mathInputButton = new MathInputButton { Input = mathKeyboardInput };
      Assert.Equal(Color.Transparent, mathInputButton.BackgroundColor);
    }
    public static IEnumerable<object[]> TheMathKeyboardInputs => Enum.GetValues(typeof(MathKeyboardInput)).Cast<MathKeyboardInput>().Select(input => new object[] { input });
  }
}
