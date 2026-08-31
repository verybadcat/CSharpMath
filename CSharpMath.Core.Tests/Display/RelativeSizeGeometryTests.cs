using CSharpMath.Atom;
using CSharpMath.Display;
using CSharpMath.Core.BackEnd;
using Xunit;
using CSharpMath.Core.AtomTests;

namespace CSharpMath.Core.DisplayTests {
  public class RelativeSizeGeometryTests {
    static readonly TestFont Font = new TestFont(10);
    static readonly Display.FrontEnd.TypesettingContext<TestFont, System.Text.Rune> Context = TestTypesettingContext.Instance;
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
    public void RelativeSizesScaleRenderedGlyph(string command, float ratio) {
      var plain = Typesetter.CreateLine(LaTeXParserTest.ParseLaTeX("a"), Font, Context, LineStyle.Text);
      var sized = Typesetter.CreateLine(LaTeXParserTest.ParseLaTeX($@"\{command} a"), Font, Context, LineStyle.Text);
      Assert.InRange(sized.Width / plain.Width, ratio - .002f, ratio + .002f);
      Assert.InRange(sized.Ascent / plain.Ascent, ratio - .01f, ratio + .01f);
      Assert.InRange(sized.Descent / plain.Descent, ratio - .01f, ratio + .01f);
    }
    [Fact]
    public void LargeScriptScalesExactlyOnce() {
      var normal = Typesetter.CreateLine(LaTeXParserTest.ParseLaTeX("x^2"), Font, Context, LineStyle.Text);
      var large = Typesetter.CreateLine(LaTeXParserTest.ParseLaTeX(@"\large x^2"), Font, Context, LineStyle.Text);
      Assert.InRange(large.Width / normal.Width, 1.19f, 1.21f);
      Assert.InRange(large.Ascent / normal.Ascent, 1.19f, 1.21f);
      Assert.InRange(large.Descent / normal.Descent, 1.19f, 1.21f);
    }
    [Theory]
    [InlineData(LineStyle.Display)]
    [InlineData(LineStyle.Text)]
    [InlineData(LineStyle.Script)]
    [InlineData(LineStyle.ScriptScript)]
    public void CubeRootDegreeUsesLegacyStyleBaseAndScalesOnce(LineStyle style) {
      var normal = Typesetter.CreateLine(LaTeXParserTest.ParseLaTeX(@"\sqrt[3]2"), Font, Context, style);
      var large = Typesetter.CreateLine(LaTeXParserTest.ParseLaTeX(@"\large \sqrt[3]2"), Font, Context, style);
      Assert.True(normal.Width > 0 && normal.Ascent > 0 && normal.Descent >= 0);
      Assert.InRange(large.Width / normal.Width, 1.19f, 1.21f);
    }
    [Theory]
    [InlineData(@"\frac{a}{b}", @"\large \frac{a}{b}")]
    [InlineData(@"\sqrt{a}", @"\large \sqrt{a}")]
    [InlineData(@"\left(a\right)", @"\large \left(a\right)")]
    [InlineData(@"\hat{a}", @"\large \hat{a}")]
    [InlineData(@"\underline{a}", @"\large \underline{a}")]
    [InlineData(@"\begin{matrix}a&b\end{matrix}", @"\large \begin{matrix}a&b\end{matrix}")]
    public void CompoundRelativeSizeScalesGeometry(string baselineLatex, string sizedLatex) {
      var baseline = Typesetter.CreateLine(LaTeXParserTest.ParseLaTeX(baselineLatex), Font, Context, LineStyle.Display);
      var sized = Typesetter.CreateLine(LaTeXParserTest.ParseLaTeX(sizedLatex), Font, Context, LineStyle.Display);
      Assert.InRange(sized.Width / baseline.Width, 1.19f, 1.21f);
      Assert.InRange(sized.Ascent / baseline.Ascent, 1.15f, 1.25f);
      Assert.InRange(sized.Descent / baseline.Descent, 1.15f, 1.25f);
    }
  }
}
