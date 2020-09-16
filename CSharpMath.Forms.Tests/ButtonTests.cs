using System;
using System.Collections.Generic;
using System.Linq;
using CSharpMath.Editor;
using Xamarin.Forms;
using Xunit;

namespace CSharpMath.Forms.Tests {
  public class ButtonTests {
    [Fact]
    public void ButtonTextColorDoesNotChangeLatexContent() {

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
    [Fact]
    public void AllMathInputButtonsHaveLatexContent() {
      foreach (var mathInputButton in TheMathInputButtons) {
        Assert.True(mathInputButton.Content != null && !string.IsNullOrEmpty(mathInputButton.Content.LaTeX));
      }
    }
    [Fact]
    public void MathInputButtonsHaveBlackTextColorByDefault() {
      foreach (var mathInputButton in TheMathInputButtons) {
        // At the time of writing this test, vphatom = @"{\color{#00FFFFFF}{|}}" (Xamarin.Forms.Color.Transparent = "#00FFFFFF") is used as there is no \vphantom command yet.
        // As soon as \vphantom has been implemented, the call .Replace(LatexHelper.vphantom, "") can be removed.
        Assert.DoesNotContain(@"\color", mathInputButton.Content!.LaTeX!.Replace(LatexHelper.vphantom, ""));
        Assert.Equal(Color.Black, mathInputButton.TextColor);
      }
    }
    [Fact]
    public void MathInputButtonsHaveTransparentBackgroundByDefault() {
      foreach (var mathInputButton in TheMathInputButtons) {
        Assert.Equal(Color.Transparent, mathInputButton.BackgroundColor);
      }
    }
    IEnumerable<MathInputButton> TheMathInputButtons => Enum.GetValues(typeof(MathKeyboardInput))
      .Cast<MathKeyboardInput>().Select(input => new MathInputButton { Input = input });
  }
}
