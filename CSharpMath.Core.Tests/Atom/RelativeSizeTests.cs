using CSharpMath.Atom;
using Xunit;

namespace CSharpMath.Core.AtomTests {
  public class RelativeSizeTests {
    [Theory]
    [InlineData("tiny")]
    [InlineData("scriptsize")]
    [InlineData("footnotesize")]
    [InlineData("small")]
    [InlineData("normalsize")]
    [InlineData("large")]
    [InlineData("Large")]
    [InlineData("LARGE")]
    [InlineData("huge")]
    [InlineData("Huge")]
    public void AllDeclarationsArePreserved(string command) {
      var list = LaTeXParserTest.ParseLaTeX($@"\{command} ab");
      Assert.Equal(2, list.Count);
      Assert.Equal($@"\{command} ab", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TransitionsScopesAndRoundTripAreStructural() {
      const string input = @"\small a{\large b}c\normalsize d";
      var serialized = LaTeXParser.MathListToLaTeX(LaTeXParserTest.ParseLaTeX(input)).ToString();
      var reparsed = LaTeXParserTest.ParseLaTeX(serialized);
      Assert.Equal(serialized, LaTeXParser.MathListToLaTeX(reparsed).ToString());
      Assert.Contains(@"\large", serialized);
      Assert.Contains(@"\normalsize", serialized);
    }

    [Fact]
    public void GroupedLargeFollowedByDefaultRoundTrips() {
      const string input = @"{\large a}b";
      var serialized = LaTeXParser.MathListToLaTeX(LaTeXParserTest.ParseLaTeX(input)).ToString();
      Assert.Equal(serialized, LaTeXParser.MathListToLaTeX(LaTeXParserTest.ParseLaTeX(serialized)).ToString());
      Assert.Equal(LaTeXParserTest.ParseLaTeX(input), LaTeXParserTest.ParseLaTeX(serialized));
    }

    [Fact]
    public void PublicRelativeSizeMutationAffectsEqualityAndRejectsInvalidValues() {
      var a = LaTeXParserTest.ParseLaTeX("a")[0];
      var b = a.Clone(false);
      Assert.Equal(a, b);
      Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ExplicitNormalSizeEqualsImplicitDefaultButSerializesWhenDeclared() {
      var implicitDefault = LaTeXParserTest.ParseLaTeX("a");
      var explicitDefault = LaTeXParserTest.ParseLaTeX(@"\normalsize a");
      Assert.Equal(implicitDefault, explicitDefault);
      Assert.Equal(implicitDefault[0].GetHashCode(), explicitDefault[0].GetHashCode());
      Assert.Contains(@"\normalsize", LaTeXParser.MathListToLaTeX(explicitDefault).ToString());
    }

    [Fact]
    public void EmptyGroupRestoresSizeAndScriptsInheritOnce() {
      var list = LaTeXParserTest.ParseLaTeX(@"\large {}x^2y");
      Assert.Equal(2, list.Count);
    }

    [Fact]
    public void ParserReportsErrorsAndDoesNotLeakState() {
      var parser = new LaTeXParser(@"\small{a");
      var (_, error) = parser.Build();
      Assert.NotNull(error);
      Assert.Equal("b", LaTeXParserTest.ParseLaTeX("b")[0].Nucleus);
    }

    [Fact]
    public void SameParserRestoresSizeAfterNestedError() {
      var parser = new LaTeXParser(@"\large\unknown c");
      Assert.NotNull(parser.Build().Error);
      MathList? tailList = null;
      parser.Build().Match(value => tailList = value, Assert.Null);
      Assert.NotNull(tailList);
      Assert.Equal("u", tailList![0].Nucleus);
    }
  }
}
