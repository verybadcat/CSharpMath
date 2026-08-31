using System.Linq;
using CSharpMath.Atom;
using CSharpMath.Atom.Atoms;
using Xunit;

namespace CSharpMath.Core.AtomTests {
  public class MulticolumnTests {
    static Table Parse(string source) {
      var result = LaTeXParser.MathListFromLaTeX(source);
      Assert.Null(result.Error);
      var (list, _) = result;
      return Assert.IsType<Table>(Assert.Single(list));
    }

    [Fact]
    public void ParsesSpanAlignmentAndRoundTrips() {
      var source = @"\begin{array}{|l|c|r|}\multicolumn{2}{l|}{a}&b\\c&d&e\end{array}";
      var table = Parse(source);
      Assert.Equal(3, table.NColumns);
      Assert.Equal(2, table.GetColumnSpan(0, 0));
      Assert.Equal(ColumnAlignment.Left, table.GetSpanAlignment(0, 0));
      Assert.Equal("l|", table.SpanSpecifications[0][0]);
      Assert.Contains(@"\multicolumn{2}{l|}", LaTeXParser.MathListToLaTeX(new MathList(table)).ToString());
      Assert.Equal(table, Parse(LaTeXParser.MathListToLaTeX(new MathList(table)).ToString()));
    }

    [Theory]
    [InlineData(@"\multicolumn{2}{c}{x}")]
    [InlineData(@"\begin{array}{cc}\multicolumn{0}{c}{x}\end{array}")]
    [InlineData(@"\begin{array}{cc}\multicolumn{3}{c}{x}\end{array}")]
    [InlineData(@"\begin{array}{cc}\multicolumn{2}{x}{x}\end{array}")]
    public void RejectsUnsupportedOrInvalidSpans(string source) {
      Assert.NotNull(LaTeXParser.MathListFromLaTeX(source).Error);
    }

    [Fact]
    public void MatrixSupportsSpans() {
      var result = LaTeXParser.MathListFromLaTeX(@"\begin{matrix}\multicolumn{2}{c}{x}&y\\z&w\end{matrix}");
      Assert.Null(result.Error);
      var (list, _) = result;
      var table = Assert.IsType<Table>(Assert.Single(list));
      Assert.Equal(2, table.GetColumnSpan(0, 0));
    }

    [Fact]
    public void SpanAlignmentAndFollowingCellsRemainDistinctAcrossRows() {
      var table = Parse(@"\begin{array}{|l|c|r|}\hline\multicolumn{2}{|c|}{x}&z\\a&\multicolumn{2}{r|}{q}\end{array}");
      Assert.Equal(3, table.NColumns);
      Assert.Equal(2, table.GetColumnSpan(0, 0));
      Assert.Equal(ColumnAlignment.Center, table.GetSpanAlignment(0, 0));
      Assert.Equal(1, table.GetColumnSpan(0, 1));
      Assert.Equal(2, table.GetColumnSpan(1, 1));
      Assert.Equal(ColumnAlignment.Right, table.GetSpanAlignment(1, 1));
      Assert.Equal(1, table.GetColumnSpan(-1, -1));
      Assert.Null(table.GetSpanAlignment(-1, -1));
    }

    [Fact]
    public void DelimitersAndHorizontalRulesComposeWithSpans() {
      var result = LaTeXParser.MathListFromLaTeX(@"\left(\begin{array}{cc}\multicolumn{2}{c}{x}\\y&z\\\hline\end{array}\right)");
      Assert.Null(result.Error);
      var (parsed, _) = result;
      var outer = Assert.IsType<Inner>(Assert.Single(parsed));
      var table = Assert.IsType<Table>(Assert.Single(outer.InnerList));
      Assert.Equal(2, table.NColumns);
      Assert.Equal(2, table.GetColumnSpan(0, 0));
      Assert.NotEmpty(table.HorizontalLines);
    }
  }
}
