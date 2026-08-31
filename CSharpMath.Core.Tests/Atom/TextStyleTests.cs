using System;
using CSharpMath.Atom;
using CSharpMath.Atom.Atoms;
using Xunit;

namespace CSharpMath.Core.AtomTests {
  public class TextStyleTests {
    [Theory]
    [InlineData(FontStyle.Default)]
    [InlineData(FontStyle.Roman)]
    [InlineData(FontStyle.Bold)]
    [InlineData(FontStyle.Caligraphic)]
    [InlineData(FontStyle.Typewriter)]
    [InlineData(FontStyle.Italic)]
    [InlineData(FontStyle.SansSerif)]
    [InlineData(FontStyle.Fraktur)]
    [InlineData(FontStyle.Blackboard)]
    [InlineData(FontStyle.BoldItalic)]
    public void LegacyStylesRoundTripExactly(FontStyle legacy) {
      var style = TextStyle.FromFontStyle(legacy);
      Assert.True(style.TryGetFontStyle(out var roundTrip));
      Assert.Equal(legacy, roundTrip);
      Assert.Equal(legacy, style.ToFontStyle());
    }

    [Fact]
    public void IndependentAxesComposeWithoutErasingEachOther() {
      var style = TextStyleChange.FromFontStyleCommand(FontStyle.SansSerif).ApplyTo(TextStyle.Default);
      style = TextStyleChange.FromFontStyleCommand(FontStyle.Bold).ApplyTo(style);
      style = style.WithPosture(FontPosture.Slanted).WithCapitals(FontCapitals.SmallCapitals);

      Assert.Equal(FontFamily.SansSerif, style.Family);
      Assert.Equal(FontWeight.Bold, style.Weight);
      Assert.Equal(FontPosture.Slanted, style.Posture);
      Assert.Equal(FontCapitals.SmallCapitals, style.Capitals);
      Assert.False(style.TryGetFontStyle(out _));
      Assert.Equal(FontStyle.SansSerif, style.ToFontStyle());
    }

    [Fact]
    public void NestedMathCommandsComposeRestoreAndRoundTrip() {
      var parsed = LaTeXParserTest.ParseLaTeX(@$"a\mathsf{{\mathbf{{\mathit{{b}}}}c}}d");

      Assert.Equal(TextStyle.Default, parsed[0].TextStyle);
      Assert.Equal(new TextStyle(FontFamily.SansSerif, FontWeight.Bold, FontPosture.Italic, FontCapitals.Normal),
        parsed[1].TextStyle);
      Assert.Equal(new TextStyle(FontFamily.SansSerif, FontWeight.Regular, FontPosture.Upright, FontCapitals.Normal),
        parsed[2].TextStyle);
      Assert.Equal(TextStyle.Default, parsed[3].TextStyle);

      var serialized = LaTeXParser.MathListToLaTeX(parsed).ToString();
      Assert.Equal(@$"a\mathsf{{\mathbf{{\mathit{{b}}}}}}\mathsf{{c}}d", serialized);
      Assert.Equal(parsed, LaTeXParserTest.ParseLaTeX(serialized));
    }

    [Fact]
    public void CloneAndEqualityPreserveSemanticRequests() {
      var original = new Variable("x") {
        TextStyle = new TextStyle(FontFamily.Roman, FontWeight.Bold, FontPosture.Slanted, FontCapitals.SmallCapitals)
      };
      var clone = original.Clone(false);
      var different = new Variable("x") { TextStyle = original.TextStyle.WithCapitals(FontCapitals.Normal) };

      Assert.Equal(original, clone);
      Assert.Equal(original.TextStyle, clone.TextStyle);
      Assert.NotEqual(original, different);
      Assert.Equal("x", original.Nucleus);
    }

    [Fact]
    public void LegacyPropertyRemainsACompatibilityLayer() {
      var atom = new Variable("x") { FontStyle = FontStyle.BoldItalic };
      Assert.Equal(FontStyle.BoldItalic, atom.FontStyle);
      Assert.Equal(FontWeight.Bold, atom.TextStyle.Weight);
      Assert.Equal(FontPosture.Italic, atom.TextStyle.Posture);
    }

    [Fact]
    public void InvalidSemanticAxesAreRejected() {
      Assert.Throws<ArgumentOutOfRangeException>(() => new TextStyle((FontFamily)99, FontWeight.Regular, FontPosture.Upright, FontCapitals.Normal));
      Assert.Throws<ArgumentOutOfRangeException>(() => TextStyle.Default.WithWeight((FontWeight)99));
      Assert.Throws<ArgumentOutOfRangeException>(() => TextStyle.Default.WithPosture((FontPosture)99));
      Assert.Throws<ArgumentOutOfRangeException>(() => TextStyle.Default.WithCapitals((FontCapitals)99));
      Assert.Throws<ArgumentOutOfRangeException>(() => new TextStyleChange((FontFamily)99, null, null, null));
      Assert.Throws<ArgumentOutOfRangeException>(() => new TextStyleChange(null, (FontWeight)99, null, null));
    }

    [Fact]
    public void StyleScopeRestoresStateAfterMalformedArgument() {
      var parser = new LaTeXParser(@"\mathbf{");
      Assert.NotNull(parser.Build().Error);
      Assert.Equal(TextStyle.Default, parser.CurrentTextStyle);
      Assert.False(parser.TextMode);
    }

    [Fact]
    public void UnsupportedAxesFailSerializationRatherThanProjecting() {
      var atom = new Variable("x") {
        TextStyle = TextStyle.Default.WithPosture(FontPosture.Slanted).WithCapitals(FontCapitals.SmallCapitals)
      };
      Assert.Throws<InvalidOperationException>(() => LaTeXParser.MathListToLaTeX(new MathList(atom)));
    }
  }
}
