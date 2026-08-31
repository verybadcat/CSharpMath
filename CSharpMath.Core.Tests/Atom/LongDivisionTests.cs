using System;
using System.Linq;
using System.Reflection;
using CSharpMath.Atom;
using CSharpMath.Atom.Atoms;
using Xunit;

namespace CSharpMath.Core.AtomTests {
  public class LongDivisionTests {
    static string CellText(MathList cell) => cell.Count == 0 ? "" : cell[0] switch {
      Number number => number.Nucleus,
      Underline underline => CellText(underline.InnerList),
      Overline overline => CellText(overline.InnerList),
      Inner inner => CellText(inner.InnerList),
      IMathListContainer container => CellText(Assert.Single(container.InnerLists)),
      _ => cell.DebugString
    };
    static LongDivision Parse(string source) {
      var result = LaTeXParser.MathListFromLaTeX(source);
      Assert.Null(result.Error);
      var (list, _) = result;
      return Assert.IsType<LongDivision>(list[0]);
    }

    [Theory]
    [InlineData("12345", "13", "949", 8)]
    [InlineData("123", "1234", "0", 123)]
    [InlineData("31415926", "2", "15707963", 0)]
    [InlineData("81", "3", "27", 0)]
    [InlineData("1132", "99", "11", 43)]
    [InlineData("86491", "94", "920", 11)]
    public void ComputesRepresentativeDivisions(string numerator, string denominator, string quotient, int remainder) {
      var atom = new LongDivision(numerator, denominator);
      Assert.Equal(quotient, atom.QuotientText);
      Assert.Equal(remainder.ToString(), atom.Remainder);
      Assert.NotEmpty(atom.Steps);
    }

    [Fact]
    public void ParsesAsSemanticAtomAndRoundTrips() {
      var atom = Parse(@"\longdiv{00123}{0007}");
      Assert.Equal("123", atom.Numerator);
      Assert.Equal("7", atom.Denominator);
      Assert.Equal(@"\longdiv{123}{7}", LaTeXParser.MathListToLaTeX(new MathList(atom)).ToString());
      Assert.Equal(atom, atom.Clone(false));
    }

    [Theory]
    [InlineData(@"\longdiv{12}{0}")]
    [InlineData(@"\longdiv{-12}{3}")]
    [InlineData(@"\longdiv{12.5}{3}")]
    [InlineData(@"\longdiv{12}{abc}")]
    [InlineData(@"\longdiv{123456789012345678901234567890}{3}")]
    public void RejectsInvalidOperands(string source) {
      var result = LaTeXParser.MathListFromLaTeX(source);
      Assert.NotNull(result.Error);
      Assert.Contains("longdiv", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ErrorDoesNotPoisonFollowingParse() {
      Assert.NotNull(LaTeXParser.MathListFromLaTeX(@"\longdiv{1}{0}").Error);
      var result = LaTeXParser.MathListFromLaTeX(@"\longdiv{81}{3}");
      Assert.Null(result.Error);
      var (list, _) = result;
      Assert.IsType<LongDivision>(list[0]);
    }

    [Theory]
    [InlineData(@"\longdiv{12}{3", "missing } for denominator", 14)]
    [InlineData(@"\longdiv{{12}}{3}", "must be a nonnegative", 14)]
    [InlineData(@"\longdiv{12}{x}", "must be a nonnegative", 15)]
    public void MalformedOperandsReportSourceContext(string source, string detail, int expectedPosition) {
      var result = LaTeXParser.MathListFromLaTeX(source);
      Assert.NotNull(result.Error);
      Assert.Contains(detail, result.Error, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("\u2191 (pos " + expectedPosition + ")", result.Error);
      Assert.Contains(source.TrimEnd('}'), result.Error);
    }

    [Fact]
    public void TraceRetainsZeroDigitsAndPlaceColumns() {
      var atom = new LongDivision("12345", "13");
      Assert.Equal(new[] { 0, 0, 9, 4, 9 }, atom.Steps.Select(s => s.QuotientDigit));
      Assert.Equal(new[] { 0, 1, 2, 3, 4 }, atom.Steps.Select(s => s.DecimalColumn));
      Assert.Equal("8", atom.Steps.Last().Remainder);
      Assert.Equal(atom.Remainder, atom.Steps.Last().Remainder);
    }

    [Theory]
    [InlineData("3", "5", "0", "3")]
    [InlineData("0", "7", "0", "0")]
    [InlineData("1005", "5", "201", "0")]
    public void HandlesShortAndExactDivisions(string numerator, string denominator, string quotient, string remainder) {
      var atom = new LongDivision(numerator, denominator);
      Assert.Equal(quotient, atom.QuotientText);
      Assert.Equal(remainder, atom.Remainder);
      Assert.Equal(numerator.Length, atom.Steps.Count);
    }

    [Fact]
    public void LayoutHasBracketRuleAndOneFinalRemainder() {
      var atom = new LongDivision("12345", "13");
      var layout = (Table)typeof(LongDivision).GetMethod("CreateLayout", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(atom, null)!;
      var dividend = layout.Cells[1][1];
      Assert.Equal("LongDivisionHeader", dividend[0].GetType().Name);
      var header = Assert.IsAssignableFrom<IMathListContainer>(dividend[0]);
      var digits = Assert.IsType<Number>(Assert.Single(Assert.Single(header.InnerLists)));
      Assert.Equal("12345", digits.Nucleus);
      Assert.Equal("949", CellText(layout.Cells[0][1]));
      Assert.Equal(ColumnAlignment.Right, layout.GetAlignment(1));
      Assert.Equal(1, layout.Cells.Count(row => row.Any(cell => CellText(cell) == atom.Remainder)));
    }

    [Fact]
    public void LayoutPreservesProductPlaceValuesAndRunningTotals() {
      var atom = new LongDivision("12345", "13");
      var layout = (Table)typeof(LongDivision).GetMethod("CreateLayout", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(atom, null)!;
      Assert.Equal(new[] { "949", "12345", "11700", "645", "520", "125", "117", "8" },
        layout.Cells.Select(row => CellText(row[1])));
    }

    [Fact]
    public void LayoutRetainsInternalZeroStepsWithoutRowsForZeroProducts() {
      var atom = new LongDivision("1005", "5");
      Assert.Contains(atom.Steps, step => step.QuotientDigit == 0);
      var layout = (Table)typeof(LongDivision).GetMethod("CreateLayout", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(atom, null)!;
      Assert.Equal("201", CellText(layout.Cells[0][1]));
      Assert.Equal(new[] { "201", "1005", "1000", "5", "5", "0" },
        layout.Cells.Select(row => CellText(row[1])));
      Assert.Equal(1, layout.Cells.Skip(2).Count(row => row.Any(cell => CellText(cell) == "0")));
    }

    [Theory]
    [InlineData("81", "3", new[] { "27", "81", "60", "21", "21", "0" })]
    [InlineData("3", "5", new[] { "0", "3", "3" })]
    [InlineData("0", "7", new[] { "0", "0", "0" })]
    [InlineData("86491", "94", new[] { "920", "86491", "84600", "1891", "1880", "11" })]
    public void LayoutShowsExactlyOneFinalRemainder(string numerator, string denominator, string[] expected) {
      var atom = new LongDivision(numerator, denominator);
      var layout = (Table)typeof(LongDivision).GetMethod("CreateLayout", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(atom, null)!;
      Assert.Equal(expected, layout.Cells.Select(row => CellText(row[1])));
      Assert.Equal(atom.Remainder, CellText(layout.Cells.Last()[1]));
    }
  }
}
