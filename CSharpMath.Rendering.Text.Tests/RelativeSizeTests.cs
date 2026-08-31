using System.Linq;
using CSharpMath.Rendering.BackEnd;
using CSharpMath.Rendering.Text;
using Xunit;

namespace CSharpMath.Rendering.Text.Tests {
  public class RelativeSizeTests {
    static readonly Fonts Font = new Fonts(Enumerable.Empty<Typography.OpenFont.Typeface>(), 20);

    static (float width, float ascent, float descent) Layout(string latex) {
      var atom = TextLaTeXParser.TextAtomFromLaTeX(latex)
        .Match(value => value, error => throw new Xunit.Sdk.XunitException(error));
      var display = TextTypesetter.Layout(atom, Font, float.PositiveInfinity).relative;
      return (display.Width, display.Ascent, display.Descent);
    }

    [Theory]
    [InlineData("tiny", .5f)]
    [InlineData("scriptsize", .7f)]
    [InlineData("footnotesize", .8f)]
    [InlineData("small", .9f)]
    [InlineData("normalsize", 1f)]
    [InlineData("large", 1.2f)]
    [InlineData("Large", 1.44f)]
    [InlineData("LARGE", 1.728f)]
    [InlineData("huge", 2.074f)]
    [InlineData("Huge", 2.488f)]
    public void AllDeclarationsPreserveCanonicalRatio(string command, float ratio) {
      var atom = TextLaTeXParser.TextAtomFromLaTeX($@"\{command} a")
        .Match(value => value, error => throw new Xunit.Sdk.XunitException(error));
      var sized = Assert.IsType<TextAtom.RelativeSize>(atom);
      Assert.Equal(command, sized.Declaration);
      Assert.Equal(ratio, sized.Magnification);
      Assert.Equal($@"\{command}{{a}}", TextLaTeXParser.TextAtomToLaTeX(atom).ToString());
    }

    [Fact]
    public void GroupScopeAndSequentialTransitionsDoNotLeak() {
      var atom = TextLaTeXParser.TextAtomFromLaTeX(@"{\small a}b\large c")
        .Match(value => value, error => throw new Xunit.Sdk.XunitException(error));
      var list = Assert.IsType<TextAtom.List>(atom);
      Assert.IsType<TextAtom.RelativeSize>(list.Content[0]);
      Assert.IsType<TextAtom.Text>(list.Content[1]);
      Assert.IsType<TextAtom.RelativeSize>(list.Content[2]);
      Assert.Equal(@"\small{a}b\large{c}", TextLaTeXParser.TextAtomToLaTeX(atom).ToString());
    }

    [Fact]
    public void RelativeSizeEqualityIncludesMagnificationAndContent() {
      var a = new TextAtom.RelativeSize(new TextAtom.Text("a"), "small");
      var b = new TextAtom.RelativeSize(new TextAtom.Text("a"), "small");
      Assert.Equal(a, b);
      Assert.Equal(a.GetHashCode(), b.GetHashCode());
      Assert.NotEqual(a, new TextAtom.RelativeSize(new TextAtom.Text("b"), "small"));
    }

    [Fact]
    public void RelativeSizesChangeActualGlyphGeometry() {
      var normal = Layout("a");
      foreach (var (command, ratio) in new[] {
        ("tiny", .5f), ("scriptsize", .7f), ("footnotesize", .8f), ("small", .9f),
        ("normalsize", 1f), ("large", 1.2f), ("Large", 1.44f), ("LARGE", 1.728f),
        ("huge", 2.074f), ("Huge", 2.488f) }) {
        var sized = Layout($@"\{command} a");
        Assert.InRange(sized.width / normal.width, ratio - .035f, ratio + .035f);
        Assert.InRange(sized.ascent / normal.ascent, ratio - .06f, ratio + .06f);
        Assert.InRange(sized.descent / normal.descent, ratio - .06f, ratio + .06f);
      }
    }

    [Fact]
    public void RelativeSizeRestoresAcrossLinesAndCoexistsWithFontSize() {
      var normal = Layout("a\\\\a");
      var mixed = Layout(@"\small a\\a \fontsize{40}{b}");
      Assert.True(mixed.width > normal.width);
      var serialized = TextLaTeXParser.TextAtomToLaTeX(
        TextLaTeXParser.TextAtomFromLaTeX(@"\small a\\a \fontsize{40}{b}")
          .Match(value => value, error => throw new Xunit.Sdk.XunitException(error))).ToString();
      Assert.Contains(@"\small", serialized);
      Assert.Contains(@"\fontsize", serialized);
    }

    [Fact]
    public void RelativeAndAbsoluteFontSizeDeclarationsHaveDefinedPrecedence() {
      var absolute = Layout(@"\fontsize{40}{a}");
      var relativeThenAbsolute = Layout(@"\small\fontsize{40}{a}");
      var absoluteThenRelative = Layout(@"\fontsize{40}{\small a}");
      Assert.InRange(relativeThenAbsolute.width / absolute.width, .995f, 1.005f);
      // Relative declarations are based on the externally supplied painter font,
      // even when nested inside an arbitrary absolute \fontsize declaration.
      Assert.InRange(absoluteThenRelative.width / absolute.width, .445f, .455f);
    }

    [Fact]
    public void RelativeSizeScalesInlineAndDisplayMath() {
      var normalInline = Layout("$x$");
      var largeInline = Layout(@"\large $x$");
      Assert.True(largeInline.width > normalInline.width);
      var normalDisplay = TextTypesetter.Layout(
        TextLaTeXParser.TextAtomFromLaTeX("$$\\frac{a}{b}$$")
          .Match(value => value, error => throw new Xunit.Sdk.XunitException(error)), Font, float.PositiveInfinity).absolute;
      var largeDisplay = TextTypesetter.Layout(
        TextLaTeXParser.TextAtomFromLaTeX(@"\large $$\frac{a}{b}$$")
          .Match(value => value, error => throw new Xunit.Sdk.XunitException(error)), Font, float.PositiveInfinity).absolute;
      Assert.True(largeDisplay.Width > normalDisplay.Width);
    }
  }
}
