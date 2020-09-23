using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpMath.Editor;
using CSharpMath.Rendering.FrontEnd;
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
    [Fact]
    public void DefaultButtonImageTextColorIsBlack() {
      var mathButton = new MathButton { Content = new MathView { LaTeX = "1" } };
      Assert.True(ImagesAreEqual(
        new FileInfo("files/buttons/BtnBlackText.png"),
        ((StreamImageSource)mathButton.Source).Stream(System.Threading.CancellationToken.None).Result));
    }
    [Fact]
    public void ButtonTextColorPropertyChangesImageColor() {
      var mathButton = new MathButton { TextColor = Color.Blue, Content = new MathView { LaTeX = "1" } };
      Assert.True(ImagesAreEqual(
        new FileInfo("files/buttons/BtnBlueText.png"),
        ((StreamImageSource)mathButton.Source).Stream(System.Threading.CancellationToken.None).Result));
    }
    static bool ImagesAreEqual(FileInfo f, Stream s) {
      using (FileStream fs = f.OpenRead()) {
        int b;
        while ((b = fs.ReadByte()) != -1) {
          if (s.ReadByte() != b) {
            return false;
          }
        }
        return fs.ReadByte() == -1;
      }
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
    [Theory]
    [MemberData(nameof(TheMathKeyboardInputs))]
    public void MathInputButton_Command(MathKeyboardInput mathKeyboardInput) {
      var mathKeyboardClassThatProcessesKeyPresses = new MathKeyboard();
      var mathInputButton = new MathInputButton { Input = mathKeyboardInput, Keyboard = mathKeyboardClassThatProcessesKeyPresses };
      mathInputButton.Command.Execute(null); // Simulate a MathInputButton key press
      Assert.Equal(expectedResult(), mathKeyboardClassThatProcessesKeyPresses.LaTeX);
      string expectedResult() {
        var kb = new MathKeyboard();
        kb.KeyPress(mathKeyboardInput);
        return kb.LaTeX;
      }
    }
    public static IEnumerable<object[]> TheMathKeyboardInputs => Enum.GetValues(typeof(MathKeyboardInput)).Cast<MathKeyboardInput>().Select(input => new object[] { input });

    [Fact]
    public void MathInputButton_KeyboardProperty() {
      var mathKeyboardClassThatProcessesKeyPresses = new MathKeyboard();
      var mathInputButton = new MathInputButton();
      mathInputButton.SetValue(MathInputButton.KeyboardProperty, mathKeyboardClassThatProcessesKeyPresses);
      Assert.Equal(mathKeyboardClassThatProcessesKeyPresses, mathInputButton.Keyboard);
    }
  }
  public class ButtonTestsWithPlatformServices {
    public ButtonTestsWithPlatformServices() => Xamarin.Forms.Device.PlatformServices = new Xamarin.Forms.Core.UnitTests.MockPlatformServices();
    const string xmlns = "xmlns=\"clr-namespace:CSharpMath.Forms;assembly=CSharpMath.Forms\"";
    [Fact]
    public void TextColorProperty_TextButton() {
      var xaml = $@"<TextButton {xmlns} TextColor=""Blue""><TextView>Text with inline $\pi$</TextView></TextButton>";
      var textButton = new TextButton().LoadFromXaml(xaml);
      Assert.Equal(Color.Blue, textButton.TextColor);
      Assert.Equal(@"Text\ with\ inline\ \(\pi \)", textButton.Content.NotNull().LaTeX);
    }
    [Fact]
    public void TextColorProperty_MathButton() {
      var xaml = $@"<MathButton {xmlns} TextColor=""Blue""><MathView>\leq</MathView></MathButton>";
      var mathButton = new MathButton().LoadFromXaml(xaml);
      Assert.Equal(Color.Blue, mathButton.TextColor);
      Assert.Equal(@"\leq ", mathButton.Content.NotNull().LaTeX);
    }
    [Fact]
    public void TextColorProperty_MathInputButton() {
      var xaml = $@"<MathInputButton {xmlns} Input=""LessOrEquals"" TextColor=""Blue"" />";
      var mathInputButton = new MathInputButton().LoadFromXaml(xaml);
      Assert.Equal(Color.Blue, mathInputButton.TextColor);
      Assert.Equal(@"\(\leq \)", mathInputButton.Content.NotNull().LaTeX);
    }
    [Fact]
    public void ImageSourceMathInputButton_InputProperty_KeyboardProperty_and_Command() {
      var mathKeyboardClassThatProcessesKeyPresses = new MathKeyboard();
      var clearXaml = $@"<ImageSourceMathInputButton {xmlns} Input=""Clear"" Source=""yourflameimage.png"" />";
      var clearButton = new ImageSourceMathInputButton().LoadFromXaml(clearXaml);
      BindKeyboardAsIfItWereViaXaml(clearButton, mathKeyboardClassThatProcessesKeyPresses);
      var aXaml = $@"<ImageSourceMathInputButton {xmlns} Input=""SmallA"" Source=""unrealisticexample.png"" />";
      var aButton = new ImageSourceMathInputButton().LoadFromXaml(aXaml);
      BindKeyboardAsIfItWereViaXaml(aButton, mathKeyboardClassThatProcessesKeyPresses);

      aButton.Command.Execute(null); // Simulate key press
      Assert.Equal("a", mathKeyboardClassThatProcessesKeyPresses.LaTeX);

      clearButton.Command.Execute(null); // Simulate key press
      Assert.Equal("", mathKeyboardClassThatProcessesKeyPresses.LaTeX);

      void BindKeyboardAsIfItWereViaXaml(ImageSourceMathInputButton imgButton, MathKeyboard mathKeyboard) =>
        imgButton.SetValue(ImageSourceMathInputButton.KeyboardProperty, mathKeyboard); // I don't know how to pass the MathKeyboard via XAML in unit tests.
    }
  }
  public static class NotNullExtension {
    public static T NotNull<T>(this T? obj) where T : class => obj ?? throw new Xunit.Sdk.NotNullException();
  }
}
