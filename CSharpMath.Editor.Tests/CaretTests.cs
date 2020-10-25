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
        InsertionPositionHighlighted = true
      };
      Assert.True(keyboard.InsertionPositionHighlighted);
      Assert.True(keyboard.ShouldDrawCaret);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + MillisecondBuffer);
      Assert.False(keyboard.InsertionPositionHighlighted);
      Assert.False(keyboard.ShouldDrawCaret);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + MillisecondBuffer);
      Assert.True(keyboard.InsertionPositionHighlighted);
      Assert.True(keyboard.ShouldDrawCaret);
    }
  }
  public class CaretStaysHidden {
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        InsertionPositionHighlighted = false
      };
      keyboard.StopBlinking();
      Assert.False(keyboard.ShouldDrawCaret);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.False(keyboard.ShouldDrawCaret);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.False(keyboard.ShouldDrawCaret);
    }
  }
  public class CaretShowsAfterAtomKeyPress {
    [Fact]
    public void Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont());
      keyboard.InsertionPositionHighlighted = false;
      keyboard.StopBlinking();
      Assert.False(keyboard.ShouldDrawCaret);
      keyboard.KeyPress(MathKeyboardInput.A);
      Assert.True(keyboard.ShouldDrawCaret);
    }
  }
  public class InsertionPositionIsHighLightedAfterPlaceholderKeyPress {
    [Fact]
    public void Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont());
      keyboard.InsertionPositionHighlighted = false;
      keyboard.StopBlinking();
      Assert.False(keyboard.InsertionPositionHighlighted);
      keyboard.KeyPress(MathKeyboardInput.Power);
      Assert.True(keyboard.InsertionPositionHighlighted);
    }
  }
  public class CaretHidesAfterReturnAndDismiss { 
    [Theory]
    [InlineData(MathKeyboardInput.Return)]
    [InlineData(MathKeyboardInput.Dismiss)]
    public async Task Test(MathKeyboardInput input) {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        InsertionPositionHighlighted = true
      };
      Assert.True(keyboard.ShouldDrawCaret);
      keyboard.KeyPress(input);
      Assert.False(keyboard.ShouldDrawCaret);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.False(keyboard.ShouldDrawCaret);
    }
  }
  public class CaretTimerResetsOnKeyPress { 
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        InsertionPositionHighlighted = true
      };
      Assert.True(keyboard.ShouldDrawCaret);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds - CaretBlinks.MillisecondBuffer);

      Assert.True(keyboard.ShouldDrawCaret);
      keyboard.KeyPress(MathKeyboardInput.A);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds - CaretBlinks.MillisecondBuffer);

      Assert.True(keyboard.ShouldDrawCaret);
      keyboard.KeyPress(MathKeyboardInput.Left);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds - CaretBlinks.MillisecondBuffer);

      Assert.True(keyboard.ShouldDrawCaret);
      await Task.Delay(4 * CaretBlinks.MillisecondBuffer);
      Assert.False(keyboard.ShouldDrawCaret);
    }
  }
  public class CaretCanStartAndStopBlinking {
    // https://github.com/verybadcat/CSharpMath/issues/115
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        InsertionPositionHighlighted = true
      };
      Assert.True(keyboard.ShouldDrawCaret);

      keyboard.StopBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.True(keyboard.ShouldDrawCaret);

      keyboard.StartBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.False(keyboard.ShouldDrawCaret);

      keyboard.StopBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.False(keyboard.ShouldDrawCaret);

      keyboard.StartBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.True(keyboard.ShouldDrawCaret);
    }
  }
  public class DefaultPlaceholderSettings {
    public const string ActiveNucleus = "■";
    public const string RestingNucleus = "□";
    public static readonly System.Drawing.Color? ActiveColor = null;
    public static readonly System.Drawing.Color? RestingColor = null;
    public const bool Blinks = false;
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
    MathKeyboard<TestFont, TGlyph> Keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont());
    public CustomizablePlaceholder() {
      LaTeXSettings.PlaceholderActiveNucleus = "😀";
      LaTeXSettings.PlaceholderRestingNucleus = "😐";
      LaTeXSettings.PlaceholderActiveColor = System.Drawing.Color.Green;
      LaTeXSettings.PlaceholderRestingColor = System.Drawing.Color.Blue;
      LaTeXSettings.PlaceholderBlinks = true;
    }
    public void Dispose() {
      LaTeXSettings.PlaceholderActiveNucleus = DefaultPlaceholderSettings.ActiveNucleus;
      LaTeXSettings.PlaceholderRestingNucleus = DefaultPlaceholderSettings.RestingNucleus;
      LaTeXSettings.PlaceholderActiveColor = DefaultPlaceholderSettings.ActiveColor;
      LaTeXSettings.PlaceholderRestingColor = DefaultPlaceholderSettings.RestingColor;
      LaTeXSettings.PlaceholderBlinks = DefaultPlaceholderSettings.Blinks;
    }
    [Fact]
    public async Task CustomizedPlaceholderBlinks() {
      Keyboard.KeyPress(MathKeyboardInput.Subscript);
      var outer = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(Keyboard.MathList));
      var inner = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(outer.Subscript));
      Assert.True(Keyboard.InsertionPositionHighlighted);
      Assert.False(Keyboard.ShouldDrawCaret);
      Assert.Equal("😐", outer.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, outer.Color);
      Assert.Equal("😀", inner.Nucleus);
      Assert.Equal(System.Drawing.Color.Green, inner.Color);

      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.False(Keyboard.InsertionPositionHighlighted);
      Assert.False(Keyboard.ShouldDrawCaret);
      Assert.Equal("😐", outer.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, outer.Color);
      Assert.Equal("😐", inner.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, inner.Color);

      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.True(Keyboard.InsertionPositionHighlighted);
      Assert.False(Keyboard.ShouldDrawCaret);
      Assert.Equal("😐", outer.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, outer.Color);
      Assert.Equal("😀", inner.Nucleus);
      Assert.Equal(System.Drawing.Color.Green, inner.Color);
    }
    [Fact]
    public void ShowRestingPlaceholdersOnCaretVisible() {
      Keyboard.KeyPress(MathKeyboardInput.Subscript);
      var outer = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(Keyboard.MathList));
      var inner = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(outer.Subscript));
      Assert.True(Keyboard.InsertionPositionHighlighted);
      Assert.False(Keyboard.ShouldDrawCaret);

      Keyboard.InsertionIndex = MathListIndex.Level0Index(Keyboard.MathList.Count);
      Assert.True(Keyboard.InsertionPositionHighlighted);
      Assert.True(Keyboard.ShouldDrawCaret);
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
  public class NonBlinkingPlaceholder : IDisposable {
    readonly MathKeyboard<TestFont, TGlyph> Keyboard;
    public NonBlinkingPlaceholder() {
      LaTeXSettings.PlaceholderBlinks = false;
      Keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContexts.Instance, new TestFont()) {
        InsertionPositionHighlighted = true
      };
    }
    public void Dispose() => LaTeXSettings.PlaceholderBlinks = DefaultPlaceholderSettings.Blinks;
    void ExpectedAppearance(string expectedOuterNucleus, string expectedInnerNucleus) {
      var outer = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(Keyboard.MathList));
      var inner = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(outer.Subscript));
      Assert.Equal(expectedOuterNucleus, outer.Nucleus);
      Assert.Equal(expectedInnerNucleus, inner.Nucleus);
    }
    [Fact]
    public async Task PlaceholderDoesNotBlinkAndNoCaretVisible() {
      Keyboard.KeyPress(MathKeyboardInput.Subscript);
      Check();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Check();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Check();
      void Check() {
        Assert.True(Keyboard.InsertionPositionHighlighted);
        Assert.False(Keyboard.ShouldDrawCaret);
        ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.ActiveNucleus);
      }
    }
    [Fact]
    public void NonBlinkingActivePlaceholderMoves() {
      Assert.True(Keyboard.InsertionPositionHighlighted);
      Assert.True(Keyboard.ShouldDrawCaret);
      Keyboard.KeyPress(MathKeyboardInput.Subscript);
      Assert.False(Keyboard.ShouldDrawCaret);
      Assert.True(Keyboard.InsertionPositionHighlighted);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.ActiveNucleus);
      Keyboard.KeyPress(MathKeyboardInput.Left);
      Assert.False(Keyboard.ShouldDrawCaret);
      Assert.True(Keyboard.InsertionPositionHighlighted);
      ExpectedAppearance(DefaultPlaceholderSettings.ActiveNucleus, DefaultPlaceholderSettings.RestingNucleus);
      Keyboard.KeyPress(MathKeyboardInput.Right);
      Assert.True(Keyboard.InsertionPositionHighlighted);
      Assert.False(Keyboard.ShouldDrawCaret);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.ActiveNucleus);
      Keyboard.KeyPress(MathKeyboardInput.Right);
      Assert.True(Keyboard.InsertionPositionHighlighted);
      Assert.True(Keyboard.ShouldDrawCaret);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.RestingNucleus);
    }
    [Fact]
    public async Task IfNonBlinkingPlaceholderIsNotHighLightedThenCaretBlinks() {
      Assert.True(Keyboard.ShouldDrawCaret);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.False(Keyboard.ShouldDrawCaret);
      Keyboard.KeyPress(MathKeyboardInput.Subscript);
      Keyboard.KeyPress(MathKeyboardInput.Right);
      Assert.True(Keyboard.ShouldDrawCaret);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.RestingNucleus);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.False(Keyboard.ShouldDrawCaret);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.RestingNucleus);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer);
      Assert.True(Keyboard.ShouldDrawCaret);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.RestingNucleus);
    }
  }
}
