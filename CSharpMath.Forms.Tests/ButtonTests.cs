using System;
using System.Collections.Generic;
using System.Linq;
using CSharpMath.Editor;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
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
  public class ButtonTestsWithPlatformServices {
    public ButtonTestsWithPlatformServices() => Xamarin.Forms.Device.PlatformServices = new Xamarin.Forms.Core.UnitTests.MockPlatformServices();
    const string xmlns = "xmlns=\"clr-namespace:CSharpMath.Forms;assembly=CSharpMath.Forms\"";
    [Fact]
    public void TextColorProperty_TextButton() {
      var xaml = $@"<TextButton {xmlns} TextColor=""Blue""><TextView {xmlns}>Text with inline $\pi$</TextView></TextButton>";
      var textButton = new TextButton().LoadFromXaml(xaml);
      Assert.Equal(Color.Blue, textButton.TextColor);
      Assert.Equal(@"Text\ with\ inline\ \(\pi \)", textButton.Content.NotNull().LaTeX);
    }
    [Fact]
    public void TextColorProperty_MathButton() {
      var xaml = $@"<MathButton {xmlns} TextColor=""Blue""><MathView {xmlns}>\leq</MathView></MathButton>";
      var mathButton = new MathButton().LoadFromXaml(xaml);
      Assert.Equal(Color.Blue, mathButton.TextColor);
      Assert.Equal(@"\leq ", mathButton.Content.NotNull().LaTeX);
    }
    [Fact]
    public void TextColorProperty_MathInputButton() {
      var xaml = $@"<MathInputButton {xmlns} Input=""LessOrEquals"" TextColor=""Blue"" />";
      var mathInputButton = new MathInputButton().LoadFromXaml(xaml);
      Assert.Equal(Color.Blue, mathInputButton.TextColor);
      Assert.Equal(@"\leq ", mathInputButton.Content.NotNull().LaTeX);
    }
  }
}
