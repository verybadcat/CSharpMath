using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
      await Task.Delay((int)MathKeyboard<TestFont, char>.DefaultBlinkMilliseconds - CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
    }
  }
}
