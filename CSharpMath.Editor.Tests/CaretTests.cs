using System;
using System.Threading.Tasks;
using CSharpMath.Atom;
using CSharpMath.CoreTests.FrontEnd;
using Xunit;
using TGlyph = System.Text.Rune;

namespace CSharpMath.Editor.Tests {
  // Tests in different classes run in parallel, unlike tests in the same class
  public class CaretBlinks {
    public const int MillisecondBuffer = 150;
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
    }
  }
  public class CaretIsOverriddenByPlaceholder {
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);

      keyboard.KeyPress(MathKeyboardInput.Power);
      var outer = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(keyboard.MathList));
      var inner = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(outer.Superscript));
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      Assert.Equal(DefaultPlaceholderSettings.RestingNucleus, outer.Nucleus);
      Assert.Equal(DefaultPlaceholderSettings.ActiveNucleus, inner.Nucleus);

      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
      Assert.Equal(DefaultPlaceholderSettings.RestingNucleus, outer.Nucleus);
      Assert.Equal(DefaultPlaceholderSettings.RestingNucleus, inner.Nucleus);

      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      Assert.Equal(DefaultPlaceholderSettings.RestingNucleus, outer.Nucleus);
      Assert.Equal(DefaultPlaceholderSettings.ActiveNucleus, inner.Nucleus);
    }
  }
  public class CaretMovesWithPlaceholder {
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);

      keyboard.KeyPress(MathKeyboardInput.Subscript);
      var outer = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(keyboard.MathList));
      var inner = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(outer.Subscript));
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      Assert.Equal(DefaultPlaceholderSettings.RestingNucleus, outer.Nucleus);
      Assert.Equal(DefaultPlaceholderSettings.ActiveNucleus, inner.Nucleus);

      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
      keyboard.KeyPress(MathKeyboardInput.Left);
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      Assert.Equal(DefaultPlaceholderSettings.ActiveNucleus, outer.Nucleus);
      Assert.Equal(DefaultPlaceholderSettings.RestingNucleus, inner.Nucleus);

      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      keyboard.KeyPress(MathKeyboardInput.Right);
      Assert.Equal(DefaultPlaceholderSettings.RestingNucleus, outer.Nucleus);
      Assert.Equal(DefaultPlaceholderSettings.ActiveNucleus, inner.Nucleus);
    }
  }
  public class CaretStaysHidden {
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Hidden
      };
      Assert.Equal(MathKeyboardCaretState.Hidden, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Hidden, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Hidden, keyboard.CaretState);
    }
  }
  public class CaretShowsAfterAtomKeyPress {
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Hidden
      };
      Assert.Equal(MathKeyboardCaretState.Hidden, keyboard.CaretState);
      keyboard.KeyPress(MathKeyboardInput.A);
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
    }
  }
  public class CaretShowsAfterPlaceholderKeyPress {
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Hidden
      };
      Assert.Equal(MathKeyboardCaretState.Hidden, keyboard.CaretState);
      keyboard.KeyPress(MathKeyboardInput.Power);
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
    }
  }
  public class CaretHidesAfterReturnAndDismiss { 
    [Theory]
    [InlineData(MathKeyboardInput.Return)]
    [InlineData(MathKeyboardInput.Dismiss)]
    public async Task Test(MathKeyboardInput input) {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      keyboard.KeyPress(input);
      Assert.Equal(MathKeyboardCaretState.Hidden, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Hidden, keyboard.CaretState);
    }
  }
  public class CaretTimerResetsOnKeyPress { 
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds - CaretBlinks.MillisecondBuffer);

      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      keyboard.KeyPress(MathKeyboardInput.A);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds - CaretBlinks.MillisecondBuffer);

      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      keyboard.KeyPress(MathKeyboardInput.Left);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds - CaretBlinks.MillisecondBuffer);

      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      await Task.Delay(4 * CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
    }
  }
  public class CaretCanStartAndStopBlinking {
    // https://github.com/verybadcat/CSharpMath/issues/115
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);

      keyboard.StopBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);

      keyboard.StartBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);

      keyboard.StopBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);

      keyboard.StartBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
    }
  }
  public class DefaultPlaceholderSettings {
    public const string ActiveNucleus = "■";
    public const string RestingNucleus = "□";
    public static readonly System.Drawing.Color? ActiveColor = null;
    public static readonly System.Drawing.Color? RestingColor = null;
    public const bool Blinks = true;
  }
  [CollectionDefinition(nameof(NonParallelPlaceholderTests), DisableParallelization = true)]
  public class NonParallelPlaceholderTests { }
  [Collection(nameof(NonParallelPlaceholderTests))]
  public class DefaultPlaceholder {
    [Fact]
    public void LaTeXSettingsPlaceholderIsNewInstance() {
      Assert.NotSame(LaTeXSettings.Placeholder, LaTeXSettings.Placeholder);
      // Double check, also verify that its contents are 'fresh':
      LaTeXSettings.Placeholder.Nucleus = "x";
      Assert.Equal(DefaultPlaceholderSettings.RestingNucleus, LaTeXSettings.Placeholder.Nucleus);
      LaTeXSettings.Placeholder.Color = System.Drawing.Color.Green;
      Assert.Equal(DefaultPlaceholderSettings.RestingColor, LaTeXSettings.Placeholder.Color);
    }
    [Fact]
    public void DefaultPlaceholderAppearance() {
      Assert.Null(LaTeXSettings.PlaceholderActiveColor);
      Assert.Null(LaTeXSettings.PlaceholderRestingColor);
      Assert.Equal(DefaultPlaceholderSettings.ActiveNucleus, LaTeXSettings.PlaceholderActiveNucleus);
      Assert.Equal(DefaultPlaceholderSettings.RestingNucleus, LaTeXSettings.PlaceholderRestingNucleus);
      Assert.Equal(LaTeXSettings.PlaceholderRestingNucleus, LaTeXSettings.Placeholder.Nucleus);
      Assert.Equal(LaTeXSettings.PlaceholderRestingColor, LaTeXSettings.Placeholder.Color);
      Assert.Equal(DefaultPlaceholderSettings.Blinks, LaTeXSettings.PlaceholderBlinks);
    }
  }
  [Collection(nameof(NonParallelPlaceholderTests))]
  public class CustomizablePlaceholder : IDisposable {
    public CustomizablePlaceholder() {
      LaTeXSettings.PlaceholderActiveNucleus = "😀";
      LaTeXSettings.PlaceholderRestingNucleus = "😐";
      LaTeXSettings.PlaceholderActiveColor = System.Drawing.Color.Green;
      LaTeXSettings.PlaceholderRestingColor = System.Drawing.Color.Blue;
    }
    public void Dispose() {
      LaTeXSettings.PlaceholderActiveNucleus = DefaultPlaceholderSettings.ActiveNucleus;
      LaTeXSettings.PlaceholderRestingNucleus = DefaultPlaceholderSettings.RestingNucleus;
      LaTeXSettings.PlaceholderActiveColor = DefaultPlaceholderSettings.ActiveColor;
      LaTeXSettings.PlaceholderRestingColor = DefaultPlaceholderSettings.RestingColor;
    }
    [Fact]
    public async Task CustomizedPlaceholderBlinks() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);

      keyboard.KeyPress(MathKeyboardInput.Subscript);
      var outer = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(keyboard.MathList));
      var inner = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(outer.Subscript));
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      Assert.Equal("😐", outer.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, outer.Color);
      Assert.Equal("😀", inner.Nucleus);
      Assert.Equal(System.Drawing.Color.Green, inner.Color);

      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, keyboard.CaretState);
      Assert.Equal("😐", outer.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, outer.Color);
      Assert.Equal("😐", inner.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, inner.Color);

      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      Assert.Equal("😐", outer.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, outer.Color);
      Assert.Equal("😀", inner.Nucleus);
      Assert.Equal(System.Drawing.Color.Green, inner.Color);
    }
    [Fact]
    public void AllCustomizablePlaceholderPropertiesAreResetOnCaretVisible() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        CaretState = MathKeyboardCaretState.Shown
      };
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      keyboard.KeyPress(MathKeyboardInput.Subscript);
      var outer = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(keyboard.MathList));
      var inner = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(outer.Subscript));
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);

      keyboard.InsertionIndex = MathListIndex.Level0Index(keyboard.MathList.Count);
      Assert.Equal(MathKeyboardCaretState.Shown, keyboard.CaretState);
      Assert.Equal(LaTeXSettings.PlaceholderRestingNucleus, outer.Nucleus);
      Assert.Equal(LaTeXSettings.PlaceholderRestingColor, outer.Color);
      Assert.Equal(LaTeXSettings.PlaceholderRestingNucleus, inner.Nucleus);
      Assert.Equal(LaTeXSettings.PlaceholderRestingColor, inner.Color);
    }
    [Fact]
    public void CustomizedPlaceholderGetter() {
      Assert.Equal("😐", LaTeXSettings.Placeholder.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, LaTeXSettings.Placeholder.Color);
    }
  }
  [Collection(nameof(NonParallelPlaceholderTests))]
  public class NonBlinkingPlaceholderViaSetting : IDisposable {
    MathKeyboard<TestFont, TGlyph> Keyboard { get; set; } = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont());
    public NonBlinkingPlaceholderViaSetting() => LaTeXSettings.PlaceholderBlinks = false;
    public void Dispose() => LaTeXSettings.PlaceholderBlinks = DefaultPlaceholderSettings.Blinks;
    void ExpectedAppearance(string expectedOuterNucleus, string expectedInnerNucleus) {
      var outer = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(Keyboard.MathList));
      var inner = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(outer.Subscript));
      Assert.Equal(expectedOuterNucleus, outer.Nucleus);
      Assert.Equal(expectedInnerNucleus, inner.Nucleus);
    }
    [Fact]
    public async Task PlaceholderDoesNotBlink() {
      Keyboard.KeyPress(MathKeyboardInput.Subscript);
      Assert.Equal(MathKeyboardCaretState.Shown, Keyboard.CaretState);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.ActiveNucleus);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Shown, Keyboard.CaretState);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.ActiveNucleus);
    }
    [Fact]
    public async Task ActivePlaceholderMoves() {
      Keyboard.KeyPress(MathKeyboardInput.Subscript);
      Keyboard.KeyPress(MathKeyboardInput.Left);
      Assert.Equal(MathKeyboardCaretState.Shown, Keyboard.CaretState);
      ExpectedAppearance(DefaultPlaceholderSettings.ActiveNucleus, DefaultPlaceholderSettings.RestingNucleus);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.Shown, Keyboard.CaretState);
      ExpectedAppearance(DefaultPlaceholderSettings.ActiveNucleus, DefaultPlaceholderSettings.RestingNucleus);
    }
    [Fact]
    public async Task CaretStillBlinks() {
      Keyboard.KeyPress(MathKeyboardInput.Subscript);
      Keyboard.KeyPress(MathKeyboardInput.Right);
      Assert.Equal(MathKeyboardCaretState.Shown, Keyboard.CaretState);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.RestingNucleus);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.Equal(MathKeyboardCaretState.TemporarilyHidden, Keyboard.CaretState);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.RestingNucleus);
    }
  }
}
