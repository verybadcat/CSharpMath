using System;
using System.Threading.Tasks;
using Xunit;
using TGlyph = System.Text.Rune;

namespace CSharpMath.Core.Tests {
  using Atom;
  using BackEnd;
  using Editor;
  // Tests in different classes run in parallel, unlike tests in the same class
  public class CaretBlinks {
    public const int MillisecondBuffer = 200;
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContext.Instance, new TestFont()) {
        InsertionPositionHighlighted = true
      };
      Assert.True(keyboard.InsertionPositionHighlighted);
      Assert.True(keyboard.ShouldDrawCaret);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.False(keyboard.InsertionPositionHighlighted);
      Assert.False(keyboard.ShouldDrawCaret);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.True(keyboard.InsertionPositionHighlighted);
      Assert.True(keyboard.ShouldDrawCaret);
    }
  }
  public class CaretStaysHidden {
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContext.Instance, new TestFont()) {
        InsertionPositionHighlighted = false
      };
      keyboard.StopBlinking();
      Assert.False(keyboard.ShouldDrawCaret);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.False(keyboard.ShouldDrawCaret);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.False(keyboard.ShouldDrawCaret);
    }
  }
  public class CaretShowsAfterAtomKeyPress {
    [Fact]
    public void Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContext.Instance, new TestFont()) {
        InsertionPositionHighlighted = false
      };
      keyboard.StopBlinking();
      Assert.False(keyboard.ShouldDrawCaret);
      keyboard.KeyPress(MathKeyboardInput.A);
      Assert.True(keyboard.ShouldDrawCaret);
    }
  }
  public class InsertionPositionIsHighLightedAfterPlaceholderKeyPress {
    [Fact]
    public void Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContext.Instance, new TestFont()) {
        InsertionPositionHighlighted = false
      };
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
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContext.Instance, new TestFont()) {
        InsertionPositionHighlighted = true
      };
      Assert.True(keyboard.ShouldDrawCaret);
      keyboard.KeyPress(input);
      Assert.False(keyboard.ShouldDrawCaret);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.False(keyboard.ShouldDrawCaret);
    }
  }
  public class CaretTimerResetsOnKeyPress {
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContext.Instance, new TestFont()) {
        InsertionPositionHighlighted = true
      };
      Assert.True(keyboard.ShouldDrawCaret);

      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds - CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.True(keyboard.ShouldDrawCaret);

      keyboard.KeyPress(MathKeyboardInput.A);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds - CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.True(keyboard.ShouldDrawCaret);

      keyboard.KeyPress(MathKeyboardInput.Left);
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds - CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.True(keyboard.ShouldDrawCaret);

      await Task.Delay(4 * CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.False(keyboard.ShouldDrawCaret);
    }
  }
  public class CaretCanStartAndStopBlinking {
    // https://github.com/verybadcat/CSharpMath/issues/115
    [Fact]
    public async Task Test() {
      var keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContext.Instance, new TestFont()) {
        InsertionPositionHighlighted = true
      };
      Assert.True(keyboard.ShouldDrawCaret);

      keyboard.StopBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.True(keyboard.ShouldDrawCaret);

      keyboard.StartBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.False(keyboard.ShouldDrawCaret);

      keyboard.StopBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.False(keyboard.ShouldDrawCaret);

      keyboard.StartBlinking();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
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
    readonly MathKeyboard<TestFont, TGlyph> keyboard;
    public CustomizablePlaceholder() {
      LaTeXSettings.PlaceholderActiveNucleus = "😀";
      LaTeXSettings.PlaceholderRestingNucleus = "😐";
      LaTeXSettings.PlaceholderActiveColor = System.Drawing.Color.Green;
      LaTeXSettings.PlaceholderRestingColor = System.Drawing.Color.Blue;
      LaTeXSettings.PlaceholderBlinks = true;
      keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContext.Instance, new TestFont());
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
      keyboard.KeyPress(MathKeyboardInput.Subscript);
      var outer = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(keyboard.MathList));
      var inner = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(outer.Subscript));
      Assert.True(keyboard.InsertionPositionHighlighted);
      Assert.False(keyboard.ShouldDrawCaret);
      Assert.Equal("😐", outer.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, outer.Color);
      Assert.Equal("😀", inner.Nucleus);
      Assert.Equal(System.Drawing.Color.Green, inner.Color);

      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.False(keyboard.InsertionPositionHighlighted);
      Assert.False(keyboard.ShouldDrawCaret);
      Assert.Equal("😐", outer.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, outer.Color);
      Assert.Equal("😐", inner.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, inner.Color);

      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.True(keyboard.InsertionPositionHighlighted);
      Assert.False(keyboard.ShouldDrawCaret);
      Assert.Equal("😐", outer.Nucleus);
      Assert.Equal(System.Drawing.Color.Blue, outer.Color);
      Assert.Equal("😀", inner.Nucleus);
      Assert.Equal(System.Drawing.Color.Green, inner.Color);
    }
    [Fact]
    public void ShowRestingPlaceholdersOnCaretVisible() {
      keyboard.KeyPress(MathKeyboardInput.Subscript);
      var outer = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(keyboard.MathList));
      var inner = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(outer.Subscript));
      Assert.True(keyboard.InsertionPositionHighlighted);
      Assert.False(keyboard.ShouldDrawCaret);

      keyboard.InsertionIndex = MathListIndex.Level0Index(keyboard.MathList.Count);
      Assert.True(keyboard.InsertionPositionHighlighted);
      Assert.True(keyboard.ShouldDrawCaret);
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
    readonly MathKeyboard<TestFont, TGlyph> keyboard;
    public NonBlinkingPlaceholder() {
      LaTeXSettings.PlaceholderBlinks = false;
      keyboard = new MathKeyboard<TestFont, TGlyph>(TestTypesettingContext.Instance, new TestFont()) {
        InsertionPositionHighlighted = true
      };
    }
    public void Dispose() => LaTeXSettings.PlaceholderBlinks = DefaultPlaceholderSettings.Blinks;
    void ExpectedAppearance(string expectedOuterNucleus, string expectedInnerNucleus) {
      var outer = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(keyboard.MathList));
      var inner = Assert.IsType<Atom.Atoms.Placeholder>(Assert.Single(outer.Subscript));
      Assert.Equal(expectedOuterNucleus, outer.Nucleus);
      Assert.Equal(expectedInnerNucleus, inner.Nucleus);
    }
    [Fact]
    public async Task PlaceholderDoesNotBlinkAndNoCaretVisible() {
      keyboard.KeyPress(MathKeyboardInput.Subscript);
      void Check() {
        Assert.True(keyboard.InsertionPositionHighlighted);
        Assert.False(keyboard.ShouldDrawCaret);
        ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.ActiveNucleus);
      }
      Check();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Check();
      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Check();
    }
    [Fact]
    public void NonBlinkingActivePlaceholderMoves() {
      Assert.True(keyboard.InsertionPositionHighlighted);
      Assert.True(keyboard.ShouldDrawCaret);

      keyboard.KeyPress(MathKeyboardInput.Subscript);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.ActiveNucleus);
      Assert.True(keyboard.InsertionPositionHighlighted);
      Assert.False(keyboard.ShouldDrawCaret);

      keyboard.KeyPress(MathKeyboardInput.Left);
      ExpectedAppearance(DefaultPlaceholderSettings.ActiveNucleus, DefaultPlaceholderSettings.RestingNucleus);
      Assert.True(keyboard.InsertionPositionHighlighted);
      Assert.False(keyboard.ShouldDrawCaret);

      keyboard.KeyPress(MathKeyboardInput.Right);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.ActiveNucleus);
      Assert.True(keyboard.InsertionPositionHighlighted);
      Assert.False(keyboard.ShouldDrawCaret);

      keyboard.KeyPress(MathKeyboardInput.Right);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.RestingNucleus);
      Assert.True(keyboard.InsertionPositionHighlighted);
      Assert.True(keyboard.ShouldDrawCaret);
    }
    [Fact]
    public async Task IfNonBlinkingPlaceholderIsNotHighLightedThenCaretBlinks() {
      Assert.True(keyboard.ShouldDrawCaret);

      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      Assert.False(keyboard.ShouldDrawCaret);

      keyboard.KeyPress(MathKeyboardInput.Subscript);
      keyboard.KeyPress(MathKeyboardInput.Right);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.RestingNucleus);
      Assert.True(keyboard.ShouldDrawCaret);

      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.RestingNucleus);
      Assert.False(keyboard.ShouldDrawCaret);

      await Task.Delay((int)MathKeyboard<TestFont, TGlyph>.DefaultBlinkMilliseconds + CaretBlinks.MillisecondBuffer, TestContext.Current.CancellationToken);
      ExpectedAppearance(DefaultPlaceholderSettings.RestingNucleus, DefaultPlaceholderSettings.RestingNucleus);
      Assert.True(keyboard.ShouldDrawCaret);
    }
  }
}