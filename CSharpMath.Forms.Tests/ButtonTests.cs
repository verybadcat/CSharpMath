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
    [ClassData(typeof(TheMathInputButtons))]
    public void AllMathInputButtonsHaveLatexContent(MathInputButton mathInputButton) {
      Assert.False(string.IsNullOrEmpty(mathInputButton.Content?.LaTeX));
    }
    [Theory]
    [ClassData(typeof(TheMathInputButtons))]
    public void MathInputButtonsHaveBlackTextColorByDefault(MathInputButton mathInputButton) {
      // At the time of writing this test, vphatom = @"{\color{#00FFFFFF}{|}}" (Xamarin.Forms.Color.Transparent = "#00FFFFFF") is used as there is no \vphantom command yet.
      // As soon as \vphantom has been implemented, the call .Replace(LatexHelper.vphantom, "") can be removed.
      Assert.DoesNotContain(@"\color", mathInputButton.Content.NotNull().LaTeX.NotNull().Replace(LatexHelper.vphantom, ""));
      Assert.Equal(Color.Black, mathInputButton.TextColor);
    }
    [Theory]
    [ClassData(typeof(TheMathInputButtons))]
    public void MathInputButtonsHaveTransparentBackgroundByDefault(MathInputButton mathInputButton) {
      Assert.Equal(Color.Transparent, mathInputButton.BackgroundColor);
    }
    public class TheMathInputButtons : TestHelpers.ComplexClassData<MathInputButton> {
      public override IEnumerable<MathInputButton> theData =>
        Enum.GetValues(typeof(MathKeyboardInput)).Cast<MathKeyboardInput>().Select(input => new MathInputButton { Input = input });
    }
  }
}
