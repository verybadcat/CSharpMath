using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpMath.Atom;
using CSharpMath.CoreTests.FrontEnd;
using Xunit;

namespace CSharpMath.Editor.Tests {
  // Tests in different classes run in parallel, unlike tests in the same class
  public class CaretBlinks {
    public const int MillisecondBuffer = 150;
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, char>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
    }
  }
  public class CaretIsOverriddenByPlaceholder {
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, char>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);

      keyboard.KeyPress(MathKeyboardInput.Power);
      var outer = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(keyboard.MathList));
      var inner = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(outer.Superscript));
      Assert.Equal(MathKeyboardCaretState.ShownThroughPlaceholder, keyboard.CaretState);
      Assert.Equal("\u25A1", outer.Nucleus);
      Assert.Equal("\u25A0", inner.Nucleus);

      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
      Assert.Equal("\u25A1", outer.Nucleus);
      Assert.Equal("\u25A1", inner.Nucleus);

      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.ShownThroughPlaceholder, keyboard.CaretState);
      Assert.Equal("\u25A1", outer.Nucleus);
      Assert.Equal("\u25A0", inner.Nucleus);
    }
  }
  public class CaretMovesWithPlaceholder {
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, char>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);

      keyboard.KeyPress(MathKeyboardInput.Subscript);
      var outer = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(keyboard.MathList));
      var inner = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(outer.Subscript));
      Assert.Equal(MathKeyboardCaretState.ShownThroughPlaceholder, keyboard.CaretState);
      Assert.Equal("\u25A1", outer.Nucleus);
      Assert.Equal("\u25A0", inner.Nucleus);

      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
      keyboard.KeyPress(MathKeyboardInput.Left);
      Assert.Equal(MathKeyboardCaretState.ShownThroughPlaceholder, keyboard.CaretState);
      Assert.Equal("\u25A0", outer.Nucleus);
      Assert.Equal("\u25A1", inner.Nucleus);

      Assert.Equal(MathKeyboardCaretState.ShownThroughPlaceholder, keyboard.CaretState);
      keyboard.KeyPress(MathKeyboardInput.Right);
      Assert.Equal("\u25A1", outer.Nucleus);
      Assert.Equal("\u25A0", inner.Nucleus);
    }
  }
  public class CaretStaysHidden {
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, char>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Hidden
      };
      Assert.Equal(MathKeyboardCaretState.Hidden, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Hidden, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Hidden, keyboard.CaretState);
    }
  }
  public class CaretShowsAfterAtomKeyPress {
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, char>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Hidden
      };
      Assert.Equal(MathKeyboardCaretState.Hidden, keyboard.CaretState);
      keyboard.KeyPress(MathKeyboardInput.A);
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
    }
  }
  public class CaretShowsAfterPlaceholderKeyPress {
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, char>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Hidden
      };
      Assert.Equal(MathKeyboardCaretState.Hidden, keyboard.CaretState);
      keyboard.KeyPress(MathKeyboardInput.Power);
      Assert.Equal(MathKeyboardCaretState.ShownThroughPlaceholder, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
    }
  }
  public class CaretHidesAfterReturnAndDismiss { 
    [Theory]
    [InlineData(MathKeyboardInput.Return)]
    [InlineData(MathKeyboardInput.Dismiss)]
    public async Task Test(MathKeyboardInput input) {
      var keyboard = new MathKeyboard<TestFont, char>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      keyboard.KeyPress(input);
      Assert.Equal(MathKeyboardCaretState.Hidden, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Hidden, keyboard.CaretState);
    }
  }
  public class CaretTimerResetsOnKeyPress { 
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, char>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds - CaretBlinks.MillisecondBuffer);

      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      keyboard.KeyPress(MathKeyboardInput.A);
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds - CaretBlinks.MillisecondBuffer);

      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      keyboard.KeyPress(MathKeyboardInput.Left);
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds - CaretBlinks.MillisecondBuffer);

      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      await Task.Delay(4 * CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
    }
  }
  public class CaretCanStartAndStopBlinking {
    // https://github.com/verybadcat/CSharpMath/issues/115
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, char>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);

      keyboard.StopBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);

      keyboard.StartBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);

      keyboard.StopBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);

      keyboard.StartBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
    }
  }
  public class CustomizablePlaceholder {
    [Fact]
    public async void Test() {
      LaTeXSettings.PlaceholderActiveNucleus = "😀";
      LaTeXSettings.PlaceholderRestingNucleus = "😐";
      LaTeXSettings.PlaceholderActiveColor = System.Drawing.Color.Green;
      LaTeXSettings.PlaceholderRestingColor = System.Drawing.Color.Blue;

      var keyboard = new MathKeyboard<TestFont, char>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);

      keyboard.KeyPress(MathKeyboardInput.Subscript);
      var outer = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(keyboard.MathList));
      var inner = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(outer.Subscript));
      Assert.Equal(MathKeyboardCaretState.ShownThroughPlaceholder, keyboard.CaretState);
      Assert.Equal("😐", outer.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, outer.Color);
      Assert.Equal("😀", inner.Nucleus);
      Assert.Equal(System.Drawing.Color.Green, inner.Color);

      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
      Assert.Equal("😐", outer.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, outer.Color);
      Assert.Equal("😐", inner.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, inner.Color);
      RestorePlaceholderDefaultSettings();
    }
    [Fact]
    public void LaTeXSettings_Placeholder_IsNewInstance() {
      Assert.False(LaTeXSettings.Placeholder == LaTeXSettings.Placeholder);
    }
    [Fact]
    public void DefaultPlaceholderAppearance() {
      Assert.Null(LaTeXSettings.PlaceholderActiveColor);
      Assert.Null(LaTeXSettings.PlaceholderRestingColor);
      Assert.Equal("\u25A0", LaTeXSettings.PlaceholderActiveNucleus);
      Assert.Equal("\u25A1", LaTeXSettings.PlaceholderRestingNucleus);
      Assert.Equal(LaTeXSettings.PlaceholderRestingNucleus, LaTeXSettings.Placeholder.Nucleus);
      Assert.Equal(LaTeXSettings.PlaceholderRestingColor, LaTeXSettings.Placeholder.Color);
    }
    [Fact]
    public void CustomizedPlaceholderGetter() {
      LaTeXSettings.PlaceholderActiveNucleus = "😀";
      LaTeXSettings.PlaceholderRestingNucleus = "😐";
      LaTeXSettings.PlaceholderActiveColor = System.Drawing.Color.Green;
      LaTeXSettings.PlaceholderRestingColor = System.Drawing.Color.Blue;
      Assert.Equal("😐", LaTeXSettings.Placeholder.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, LaTeXSettings.Placeholder.Color);
      RestorePlaceholderDefaultSettings();
    }
    private void RestorePlaceholderDefaultSettings() {
      LaTeXSettings.PlaceholderActiveNucleus = "\u25A0";
      LaTeXSettings.PlaceholderRestingNucleus = "\u25A1";
      LaTeXSettings.PlaceholderActiveColor = null;
      LaTeXSettings.PlaceholderRestingColor = null;
    }
  }
}
