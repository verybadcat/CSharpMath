using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using CSharpMath.Atoms;
using CSharpMath.Atoms.Atom;

namespace CSharpMath.Tests.PreTypesetting {

  public class LaTeXBuilderTest {
    public static IEnumerable<(string, Type[], string)> RawTestData() => new[] {
      ("x", new[] { typeof(Variable) }, "x"),
      ("1", new[] { typeof(Number) }, "1"),
      ("*", new[] { typeof(BinaryOperator) }, "*"),
      ("+", new[] { typeof(BinaryOperator) }, "+"),
      (".", new[] { typeof(Number) }, "."),
      ("(", new[] { typeof(Open) }, "("),
      (")", new[] { typeof(Close) }, ")"),
      (",", new[] { typeof(Punctuation) }, ","),
      ("!", new[] { typeof(Close) }, "!"),
      ("=", new[] { typeof(Relation) }, "="),
      ("x+2", new[] { typeof(Variable), typeof(BinaryOperator), typeof(Number) }, "x+2"),
      ("(2.3 * 8)", new[] { typeof(Open), typeof(Number), typeof(Number), typeof(Number), typeof(BinaryOperator), typeof(Number), typeof(Close) }, "(2.3*8)"),
      ("5{3+4}", new[] { typeof(Number), typeof(Number), typeof(BinaryOperator), typeof(Number) }, "53+4"), // braces are just for grouping
      // commands
      (@"\pi+\theta\geq 3", new[] { typeof(Variable), typeof(BinaryOperator), typeof(Variable), typeof(Relation), typeof(Number) }, @"\pi +\theta \geq 3"),
      // aliases
      (@"\pi\ne 5 \land 3", new[] { typeof(Variable), typeof(Relation), typeof(Number), typeof(BinaryOperator), typeof(Number) }, @"\pi \neq 5\wedge 3"),
      // control space
      (@"x \ y", new[] { typeof(Variable), typeof(Ordinary), typeof(Variable) }, @"x\  y"),
      // spacing
      (@"x \quad y \; z \! q", new[] { typeof(Variable), typeof(Space), typeof(Variable), typeof(Space), typeof(Variable), typeof(Space), typeof(Variable)}, @"x\quad y\; z\! q")
    };

    public static IEnumerable<(string, Type[][], string)> RawSuperscriptTestData() => new[] {
      ("x^2", new[] { new[] { typeof(Variable) }, new[] { typeof(Number) } }, "x^2"),
      ("x^23", new[] { new[] { typeof(Variable), typeof(Number) }, new[] { typeof(Number) } }, "x^23"),
      ("x^{23}", new[] { new[] { typeof(Variable) }, new[] { typeof(Number), typeof(Number) } }, "x^{23}"),
      ("x^2^3", new[] { new[] { typeof(Variable), typeof(Ordinary) }, new[] { typeof(Number) } }, "x^2{}^3"),
      ("x^{2^3}", new[] { new[] { typeof(Variable)}, new[] { typeof(Number) }, new[] {typeof(Number)} }, "x^{2^3}"),
      ("x^{^2*}", new[] { new[] { typeof(Variable) }, new[] { typeof(Ordinary), typeof(BinaryOperator) }, new[] { typeof(Number) } }, "x^{{}^2*}"),
      ("^2", new[] { new[] { typeof(Ordinary) }, new[] { typeof(Number) } }, "{}^2"),
      ("{}^2", new[] { new[] { typeof(Ordinary) }, new[] { typeof(Number) } }, "{}^2"),
      ("x^^2", new[] { new[] { typeof(Variable), typeof(Ordinary) }, new Type[] { } }, "x^{}{}^2"),
      ("5{x}^2", new[] { new[] { typeof(Number), typeof(Variable) }, new Type[] { } }, "5x^2")
    };

    public static IEnumerable<object[]> SuperscriptTestData() {
      foreach (var (i1, i2, i3) in RawSuperscriptTestData())
        yield return new object[] { i1, i2, i3 };
    }

    public static IEnumerable<object[]> SubscriptTestData() {
      foreach (var (i1, i2, i3) in RawSuperscriptTestData())
        yield return new object[] { i1.Replace('^', '_'), i2, i3.Replace('^', '_') };
    }

    public static IEnumerable<object[]> TestData() {
      foreach (var (i1, i2, i3) in RawTestData())
        yield return new object[] { i1, i2, i3 };
    }

    [Theory, MemberData(nameof(TestData))]
    public void TestBuilder(string input, Type[] atomTypes, string output) {
      var builder = new LaTeXBuilder(input);
      var list = builder.Build();
      Assert.Null(builder.Error);

      CheckAtomTypes(list, atomTypes);

      Assert.Equal(output, LaTeXBuilder.MathListToLaTeX(list));
    }

    [Theory, MemberData(nameof(SuperscriptTestData))]
    public void TestSuperscript(string input, Type[][] atomTypes, string output) =>
      RunScriptTest(input, atom => atom.Superscript, atomTypes, output);

    [Theory, MemberData(nameof(SubscriptTestData))]
    public void TestSubscript(string input, Type[][] atomTypes, string output) =>
      RunScriptTest(input, atom => atom.Subscript, atomTypes, output);

    private void RunScriptTest
      (string input, Func<MathAtom, MathList> scriptGetter, Type[][] atomTypes, string output) {
      var builder = new LaTeXBuilder(input);
      var list = builder.Build();
      Assert.Null(builder.Error);

      var expandedList = list.Clone(false);
      CheckAtomTypes(expandedList, atomTypes[0]);

      var firstAtom = expandedList[0];
      var types = atomTypes[1];
      if (types.Length > 0)
        Assert.NotNull(scriptGetter(firstAtom));

      var scriptList = scriptGetter(firstAtom);
      CheckAtomTypes(scriptList, atomTypes[1]);
      if (atomTypes.Length == 3) {
        // one more level
        var firstScript = scriptList[0];
        var scriptScriptList = scriptGetter(firstScript);
        CheckAtomTypes(scriptScriptList, atomTypes[2]);
      }

      string latex = LaTeXBuilder.MathListToLaTeX(list);
      Assert.Equal(output, latex);
    }

    /// <summary>Safe to call with a null list. Types cannot be null however.</summary>
    private void CheckAtomTypes(MathList list, params Type[] types) {
      int atomCount = (list == null) ? 0 : list.Atoms.Count;
      Assert.Equal(types.Length, atomCount);
      for (int i = 0; i < atomCount; i++) {
        var atom = list[i];
        Assert.NotNull(atom);
        Assert.IsType(types[i], atom);
      }
    }

    private Action<MathAtom> CheckAtom<T>
      (string nucleus, Action<T> action = null) where T : MathAtom =>
      atom => {
        var actualAtom = Assert.IsType<T>(atom);
        Assert.Equal(nucleus, actualAtom.Nucleus);
        action?.Invoke(actualAtom);
      };

    [Fact]
    public void TestSymbols() {
      var list = new LaTeXBuilder(@"5\times3^{2\div2}").Build();
      Assert.Collection(list,
        CheckAtom<Number>("5"),
        CheckAtom<BinaryOperator>("\u00D7"),
        CheckAtom<Number>("3", three =>
          Assert.Collection(three.Superscript,
            CheckAtom<Number>("2"),
            CheckAtom<BinaryOperator>("\u00F7"),
            CheckAtom<Number>("2")
          )
        )
      );
    }

    [Fact]
    public void TestFraction() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\frac1c");
      Assert.Collection(list,
        CheckAtom<Fraction>("", fraction => {
          Assert.True(fraction.HasRule);
          Assert.Null(fraction.LeftDelimiter);
          Assert.Null(fraction.RightDelimiter);
          Assert.Collection(fraction.Numerator, CheckAtom<Number>("1"));
          Assert.Collection(fraction.Denominator, CheckAtom<Variable>("c"));
        })
      );
      Assert.Equal(@"\frac{1}{c}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestFractionInFraction() {
      var input = @"\frac1\frac23";
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Collection(list,
        CheckAtom<Fraction>("", fraction => {
          Assert.Collection(fraction.Numerator, CheckAtom<Number>("1"));
          Assert.Collection(fraction.Denominator,
            CheckAtom<Fraction>("", subFraction => {
              Assert.Collection(subFraction.Numerator, CheckAtom<Number>("2"));
              Assert.Collection(subFraction.Denominator, CheckAtom<Number>("3"));
            })
          );
        })
      );
      Assert.Equal(@"\frac{1}{\frac{2}{3}}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestSqrt() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\sqrt2");
      Assert.Collection(list,
        CheckAtom<Radical>("", radical => {
          Assert.Null(radical.Degree);
          Assert.Collection(radical.Radicand, CheckAtom<Number>("2"));
        })
      );
      Assert.Equal(@"\sqrt{2}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestSqrtInSqrt() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\sqrt\sqrt2");
      Assert.Collection(list,
        CheckAtom<Radical>("", radical =>
          Assert.Collection(radical.Radicand,
            CheckAtom<Radical>("", subRadical =>
              Assert.Collection(subRadical.Radicand, CheckAtom<Number>("2"))
            )
          )
        )
      );
      Assert.Equal(@"\sqrt{\sqrt{2}}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestRadical() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\sqrt[3]2");
      Assert.Collection(list,
        CheckAtom<Radical>("", radical => {
          Assert.Collection(radical.Degree, CheckAtom<Number>("3"));
          Assert.Collection(radical.Radicand, CheckAtom<Number>("2"));
        })
      );
      Assert.Equal(@"\sqrt[3]{2}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [
      Theory,
      InlineData(@"\left( 2 \right)", new[] { typeof(Inner) }, new[] { typeof(Number) }, @"(", @")", @"\left( 2\right) "),
      // spacing
      InlineData(@"\left ( 2 \right )", new[] { typeof(Inner) }, new[] { typeof(Number) }, @"(", @")", @"\left( 2\right) "),
      // commands
      InlineData(@"\left\{ 2 \right\}", new[] { typeof(Inner) }, new[] { typeof(Number) }, @"{", @"}", @"\left\{ 2\right\} "),
      // complex commands
      InlineData(@"\left\langle x \right\rangle", new[] { typeof(Inner) }, new[] { typeof(Variable) }, "\u2329", "\u232A", @"\left< x\right> "),
      // bars
      InlineData(@"\left| x \right\|", new[] { typeof(Inner) }, new[] { typeof(Variable) }, @"|", "\u2016", @"\left| x\right\| "),
      // inner in between
      InlineData(@"5 + \left( 2 \right) - 2", new[] { typeof(Number), typeof(BinaryOperator), typeof(Inner), typeof(BinaryOperator), typeof(Number) }, new[] { typeof(Number) }, @"(", @")", @"5+\left( 2\right) -2"),
      // long inner
      InlineData(@"\left( 2 + \frac12\right)", new[] { typeof(Inner) }, new[] { typeof(Number), typeof(BinaryOperator), typeof(Fraction) }, @"(", @")", @"\left( 2+\frac{1}{2}\right) "),
      // nested
      InlineData(@"\left[ 2 + \left|\frac{-x}{2}\right| \right]", new[] { typeof(Inner) }, new[] { typeof(Number), typeof(BinaryOperator), typeof(Inner) }, @"[", @"]", @"\left[ 2+\left| \frac{-x}{2}\right| \right] "),
      // With scripts
      InlineData(@"\left( 2 \right)^2", new[] { typeof(Inner) }, new[] { typeof(Number) }, @"(", @")", @"\left( 2\right) ^2"),
      // Scripts on left
      InlineData(@"\left(^2 \right )", new[] { typeof(Inner) }, new[] { typeof(Ordinary) }, @"(", @")", @"\left( {}^2\right) "),
      // Dot
      InlineData(@"\left( 2 \right.", new[] { typeof(Inner) }, new[] { typeof(Number) }, @"(", @"", @"\left( 2\right. ")
    ]
    public void TestLeftRight(
      string input, Type[] expectedOutputTypes, Type[] expectedInnerTypes,
      string leftBoundary, string rightBoundary, string expectedLatex) {
      var builder = new LaTeXBuilder(input);
      var list = builder.Build();
      Assert.NotNull(list);
      Assert.Null(builder.Error);

      CheckAtomTypes(list, expectedOutputTypes);
      Assert.Single(expectedOutputTypes, t => t == typeof(Inner));
      CheckAtom<Inner>("", inner => {
        CheckAtomTypes(inner.InnerList, expectedInnerTypes);
        Assert.Equal(leftBoundary, inner.LeftBoundary?.Nucleus);
        Assert.Equal(rightBoundary, inner.RightBoundary?.Nucleus);
      })(list[Array.IndexOf(expectedOutputTypes, typeof(Inner))]);
      Assert.Equal(expectedLatex, LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestOver() {
      var input = @"1 \over c";
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Single(list);

      var fraction = list[0] as Fraction;
      CheckAtom<Fraction>("")(fraction);
      Assert.True(fraction.HasRule);
      Assert.Null(fraction.LeftDelimiter);
      Assert.Null(fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtom<Number>("1")(fraction.Numerator[0]);

      Assert.Single(fraction.Denominator);
      CheckAtom<Variable>("c")(fraction.Denominator[0]);

      Assert.Equal(@"\frac{1}{c}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestOverInParens() {
      var input = @"5 + {1 \over c} + 8";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.NotNull(list);
      Assert.Equal(5, list.Count);
      var types = new Type[] { typeof(Number), typeof(BinaryOperator), typeof(Fraction), typeof(BinaryOperator), typeof(Number) };
      CheckAtomTypes(list, types);

      var fraction = list[2] as Fraction;
      CheckAtom<Fraction>("")(fraction);
      Assert.True(fraction.HasRule);
      Assert.Null(fraction.LeftDelimiter);
      Assert.Null(fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtom<Number>("1")(fraction.Numerator[0]);
      Assert.Single(fraction.Denominator);
      CheckAtom<Variable>("c")(fraction.Denominator[0]);

      Assert.Equal(@"5+\frac{1}{c}+8", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestAtop() {
      var input = @"1 \atop c";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var fraction = list[0] as Fraction;
      CheckAtom<Fraction>("")(fraction);
      Assert.False(fraction.HasRule);
      Assert.Null(fraction.LeftDelimiter);
      Assert.Null(fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtom<Number>("1")(fraction.Numerator[0]);

      Assert.Single(fraction.Denominator);
      CheckAtom<Variable>("c")(fraction.Denominator[0]);

      Assert.Equal(@"{1 \atop c}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestAtopInParens() {
      var input = @"5 + {1 \atop c} + 8";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Equal(5, list.Count);
      var types = new Type[] {
        typeof(Number),
        typeof(BinaryOperator),
        typeof(Fraction),
        typeof(BinaryOperator),
        typeof(Number)
      };
      CheckAtomTypes(list, types);
      var fraction = list[2] as Fraction;
      CheckAtom<Fraction>("")(fraction);
      Assert.False(fraction.HasRule);
      Assert.Null(fraction.LeftDelimiter);
      Assert.Null(fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtom<Number>("1")(fraction.Numerator[0]);

      Assert.Single(fraction.Denominator);
      CheckAtom<Variable>("c")(fraction.Denominator[0]);

      Assert.Equal(@"5+{1 \atop c}+8", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestChoose() {
      var input = @"n \choose k";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      CheckAtom<Fraction>("")(list[0]);

      var fraction = list[0] as Fraction;
      Assert.False(fraction.HasRule);
      Assert.Equal("(", fraction.LeftDelimiter);
      Assert.Equal(")", fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtom<Variable>("n")(fraction.Numerator[0]);

      Assert.Single(fraction.Denominator);
      CheckAtom<Variable>("k")(fraction.Denominator[0]);
      Assert.Equal(@"{n \choose k}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestBrack() {
      var input = @"n \brack k";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var fraction = list[0] as Fraction;
      CheckAtom<Fraction>("")(fraction);

      Assert.False(fraction.HasRule);
      Assert.Equal("[", fraction.LeftDelimiter);
      Assert.Equal("]", fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtom<Variable>("n")(fraction.Numerator[0]);
      Assert.Single(fraction.Denominator);
      CheckAtom<Variable>("k")(fraction.Denominator[0]);

      Assert.Equal(@"{n \brack k}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestBrace() {
      var input = @"n \brace k";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var fraction = list[0] as Fraction;
      CheckAtom<Fraction>("")(fraction);

      Assert.False(fraction.HasRule);
      Assert.Equal("{", fraction.LeftDelimiter);
      Assert.Equal("}", fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtom<Variable>("n")(fraction.Numerator[0]);
      Assert.Single(fraction.Denominator);
      CheckAtom<Variable>("k")(fraction.Denominator[0]);

      Assert.Equal(@"{n \brace k}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestBinomial() {
      var input = @"\binom{n}{k}";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var fraction = list[0] as Fraction;
      CheckAtom<Fraction>("")(fraction);

      Assert.False(fraction.HasRule);
      Assert.Equal("(", fraction.LeftDelimiter);
      Assert.Equal(")", fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtom<Variable>("n")(fraction.Numerator[0]);
      Assert.Single(fraction.Denominator);
      CheckAtom<Variable>("k")(fraction.Denominator[0]);

      Assert.Equal(@"{n \choose k}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestOverline() {
      var input = @"\overline 2";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var overline = list[0] as Overline;
      CheckAtom<Overline>("")(overline);

      var inner = overline.InnerList;
      Assert.Single(inner);
      CheckAtom<Number>("2")(inner[0]);

      Assert.Equal(@"\overline{2}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestUnderline() {
      var input = @"\underline 2";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var underline = list[0] as Underline;
      CheckAtom<Underline>("")(underline);

      var inner = underline.InnerList;
      Assert.Single(inner);
      CheckAtom<Number>("2")(inner[0]);

      Assert.Equal(@"\underline{2}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestAccent() {
      var input = @"\bar x";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var accent = list[0] as Accent;
      CheckAtom<Accent>("\u0304")(accent);

      var inner = accent.InnerList;
      Assert.Single(inner);
      CheckAtom<Variable>("x")(inner[0]);

      Assert.Equal(@"\bar{x}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestMathSpace() {
      var input = @"\!";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      CheckAtom<Space>("")(list[0]);
      Assert.Equal(-3, (list[0] as Space).Length);

      Assert.Equal(@"\! ", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestMathStyle() {
      var input = @"\textstyle y \scriptstyle x";
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Equal(4, list.Count);

      var style = list[0] as Style;
      CheckAtom<Style>("")(style);
      Assert.Equal(LineStyle.Text, style.LineStyle);

      var style2 = list[2] as Style;
      CheckAtom<Style>("")(style2);
      Assert.Equal(LineStyle.Script, style2.LineStyle);

      Assert.Equal(@"\textstyle y\scriptstyle x", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestMatrix() {
      var input = @"\begin{matrix} x & y \\ z & w \end{matrix}";
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var table = list[0] as Table;
      CheckAtom<Table>("")(table);
      Assert.Equal("matrix", table.Environment);
      Assert.Equal(0, table.InterRowAdditionalSpacing);
      Assert.Equal(18, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(2, table.NColumns);

      for (int i = 0; i < 2; i++) {
        var alignment = table.GetAlignment(i);
        Assert.Equal(ColumnAlignment.Center, alignment);
        for (int j = 0; j < 2; j++) {
          var cell = table.Cells[j][i];
          Assert.Equal(2, cell.Count);
          var style = cell[0] as Style;
          Assert.IsType<Style>(style);
          Assert.Equal(LineStyle.Text, style.LineStyle);

          var atom = cell[1];
          Assert.IsType<Variable>(atom);
        }
      }
      Assert.Equal(@"\begin{matrix}x&y\\ z&w\end{matrix}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestPMatrix() {
      var input = @"\begin{pmatrix} x & y \\ z & w \end{pmatrix}";
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var inner = list[0] as Inner;
      CheckAtom<Inner>("")(inner);
      var innerList = inner.InnerList;
      Assert.Equal("(", inner.LeftBoundary?.Nucleus);
      Assert.Equal(")", inner.RightBoundary?.Nucleus);
      Assert.Single(innerList);
      var table = innerList[0] as Table;
      CheckAtom<Table>("")(table);
      Assert.Equal("matrix", table.Environment);
      Assert.Equal(0, table.InterRowAdditionalSpacing);
      Assert.Equal(18, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(2, table.NColumns);

      for (int i = 0; i < 2; i++) {
        var alignment = table.GetAlignment(i);
        Assert.Equal(ColumnAlignment.Center, alignment);
        for (int j = 0; j < 2; j++) {
          var cell = table.Cells[j][i];
          Assert.Equal(2, cell.Count);
          var style = cell[0] as Style;
          Assert.IsType<Style>(style);
          Assert.Equal(LineStyle.Text, style.LineStyle);

          var atom = cell[1];
          Assert.IsType<Variable>(atom);
        }
      }
      Assert.Equal(@"\left( \begin{matrix}x&y\\ z&w\end{matrix}\right) ", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestDefaultTable() {
      var input = @"x \\ y";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var table = list[0] as Table;
      CheckAtom<Table>("")(table);
      Assert.Null(table.Environment);
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(0, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(1, table.NColumns);
      for (int i = 0; i < 1; i++) {
        Assert.Equal(ColumnAlignment.Left, table.GetAlignment(i));
        for (int j = 0; j < 2; j++) {
          var cell = table.Cells[j][i];
          Assert.Single(cell);
          Assert.IsType<Variable>(cell[0]);
        }
      }
      Assert.Equal(@"x\\ y", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestTableWithColumns() {
      var input = @"x & y \\ z & w";
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var table = list[0] as Table;
      CheckAtom<Table>("")(table);
      Assert.Null(table.Environment);
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(0, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(2, table.NColumns);

      for (int i = 0; i < 2; i++) {
        Assert.Equal(ColumnAlignment.Left, table.GetAlignment(i));
        for (int j = 0; j < 2; j++) {
          var cell = table.Cells[j][i];
          Assert.Single(cell);
          Assert.IsType<Variable>(cell[0]);
        }
      }

      Assert.Equal(@"x&y\\ z&w", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Theory]
    [InlineData(@"\begin{eqalign}x&y\\ z&w\end{eqalign}")]
    [InlineData(@"\begin{split}x&y\\ z&w\end{split}")]
    [InlineData(@"\begin{aligned}x&y\\ z&w\end{aligned}")]
    public void TestEqAlign(string input) {
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var table = list[0] as Table;
      CheckAtom<Table>("")(table);
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(0, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(2, table.NColumns);
      for (int i = 0; i < 2; i++) {
        var alignment = table.GetAlignment(i);
        Assert.Equal(i == 0 ? ColumnAlignment.Right : ColumnAlignment.Left, alignment);
        for (int j = 0; j < 2; j++) {
          var cell = table.Cells[j][i];
          if (i == 0) {
            Assert.Single(cell);
            Assert.IsType<Variable>(cell[0]);
          } else {
            Assert.Equal(2, cell.Count);
            Assert.IsType<Ordinary>(cell[0]);
            Assert.IsType<Variable>(cell[1]);
          }
        }
      }
      Assert.Equal(input, LaTeXBuilder.MathListToLaTeX(list));
    }

    [Theory]
    [InlineData(@"\begin{displaylines}x\\ y\end{displaylines}")]
    [InlineData(@"\begin{gather}x\\ y\end{gather}")]
    public void TestDisplayLines(string input) {
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var table = list[0] as Table;
      CheckAtom<Table>("")(table);
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(0, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(1, table.NColumns);
      Assert.Equal(ColumnAlignment.Center, table.GetAlignment(0));
      for (int j = 0; j < 2; j++) {
        var cell = table.Cells[j][0];
        Assert.Single(cell);
        Assert.IsType<Variable>(cell[0]);
      }
      Assert.Equal(input, LaTeXBuilder.MathListToLaTeX(list));
    }

    [Theory]
    [InlineData(@"}a")]
    [InlineData(@"\notacommand")]
    [InlineData(@"\sqrt[5+3")]
    [InlineData(@"{5+3")]
    [InlineData(@"5+3}")]
    [InlineData(@"{1+\frac{3+2")]
    [InlineData(@"1+\left")]
    [InlineData(@"\left(\frac12\right")]
    [InlineData(@"\left 5 + 3 \right)")]
    [InlineData(@"\left(\frac12\right + 3")]
    [InlineData(@"\left\lmoustache 5 + 3 \right)")]
    [InlineData(@"\left(\frac12\right\rmoustache + 3")]
    [InlineData(@"5 + 3 \right)")]
    [InlineData(@"\left(\frac12")]
    [InlineData(@"\left(5 + \left| \frac12 \right)")]
    [InlineData(@"5+ \left|\frac12\right| \right)")]
    [InlineData(@"\begin matrix \end matrix")] // missing {
    [InlineData(@"\begin")] // missing {
    [InlineData(@"\begin{")] // missing }
    [InlineData(@"\begin{matrix parens}")] // missing } (no spaces in env)
    [InlineData(@"\begin{matrix} x")]
    [InlineData(@"\begin{matrix} x \end")] // missing {
    [InlineData(@"\begin{matrix} x \end + 3")] // missing {
    [InlineData(@"\begin{matrix} x \end{")] // missing }
    [InlineData(@"\begin{matrix} x \end{matrix + 3")]// missing }
    [InlineData(@"\begin{matrix} x \end{pmatrix}")]
    [InlineData(@"x \end{matrix}")]
    [InlineData(@"\begin{notanenv} x \end{notanenv}")]
    [InlineData(@"\begin{matrix} \notacommand \end{matrix}")]
    [InlineData(@"\begin{displaylines} x & y \end{displaylines}")]
    [InlineData(@"\begin{eqalign} x \end{eqalign}")]
    [InlineData(@"\nolimits")]
    [InlineData(@"\frac\limits{1}{2}")]
    public void TestErrors(string badInput) {
      var builder = new LaTeXBuilder(badInput);
      var list = builder.Build();
      Assert.Null(list);
      Assert.NotNull(builder.Error);
    }

    [Fact]
    public void TestCustom() {
      var input = @"\lcm(a,b)";
      var builder = new LaTeXBuilder(input);
      var list = builder.Build();
      Assert.Null(list);
      Assert.NotNull(builder.Error);

      MathAtoms.AddLatexSymbol("lcm", new LargeOperator("lcm", false));
      var builder2 = new LaTeXBuilder(input);
      var list2 = builder2.Build();
      CheckAtomTypes(list2, typeof(LargeOperator), typeof(Open),
        typeof(Variable), typeof(Punctuation), typeof(Variable),
        typeof(Close));
      var latex = LaTeXBuilder.MathListToLaTeX(list2);
      Assert.Equal(@"\lcm (a,b)", latex);
    }

    [Fact]
    public void TestFontSingle() {
      var input = @"\mathbf x";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      CheckAtom<Variable>("x")(list[0]);
      Assert.Equal(FontStyle.Bold, list[0].FontStyle);

      Assert.Equal(@"\mathbf{x}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestFontMultipleCharacters() {
      var input = @"\frak{xy}";
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Equal(2, list.Count);
      CheckAtom<Variable>("x")(list[0]);
      Assert.Equal(FontStyle.Fraktur, list[0].FontStyle);
      CheckAtom<Variable>("y")(list[1]);
      Assert.Equal(FontStyle.Fraktur, list[1].FontStyle);

      Assert.Equal(@"\mathfrak{xy}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestFontOneCharacterInside() {
      var input = @"\sqrt \mathrm x y";
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Equal(2, list.Count);

      var radical = list[0] as Radical;
      CheckAtom<Radical>("")(radical);

      var sublist = radical.Radicand;
      var atom = sublist[0];
      CheckAtom<Variable>("x")(atom);
      Assert.Equal(FontStyle.Roman, atom.FontStyle);

      CheckAtom<Variable>("y")(list[1]);
      Assert.Equal(FontStyle.Default, list[1].FontStyle);

      Assert.Equal(@"\sqrt{\mathrm{x}}y", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestText() {
      var input = @"\text{x y}";
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Equal(3, list.Count);
      CheckAtom<Variable>(@"x")(list[0]);
      Assert.Equal(FontStyle.Roman, list[0].FontStyle);

      CheckAtom<Ordinary>(" ")(list[1]);
      CheckAtom<Variable>(@"y")(list[2]);
      Assert.Equal(FontStyle.Roman, list[2].FontStyle);

      Assert.Equal(@"\mathrm{x\  y}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestLimits() {
      var input = @"\int";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var op = list[0] as LargeOperator;
      Assert.IsType<LargeOperator>(op);
      Assert.False(op.Limits);
      Assert.Equal(@"\int ", LaTeXBuilder.MathListToLaTeX(list));

      var input2 = @"\int\limits";
      var list2 = LaTeXBuilder.MathListFromLaTeX(input2);
      Assert.Single(list2);
      var op2 = list2[0] as LargeOperator;
      Assert.IsType<LargeOperator>(op2);
      Assert.True(op2.Limits);

      var latex2 = LaTeXBuilder.MathListToLaTeX(list2);
      Assert.Equal(@"\int \limits ", latex2);
    }

    [Fact]
    public void TestUnspecifiedLimits() {
      var input = @"\sum";
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var op = list[0] as LargeOperator;
      Assert.IsType<LargeOperator>(op);
      Assert.Null(op.Limits);
      Assert.Equal(@"\sum ", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestNoLimits() {
      var input = @"\sum\nolimits";
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var op = list[0] as LargeOperator;
      Assert.IsType<LargeOperator>(op);
      Assert.False(op.Limits);
      Assert.Equal(@"\sum \nolimits ", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestColor() {
      var input = @"\color{#F00}a";
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var op = list[0] as Color;
      Assert.IsType<Color>(op);
      Assert.False(op.ScriptsAllowed);
      Assert.Equal(@"\color{#F00}{a}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestSingleColumnArray() {
      var input = @"\begin{array}{l}a=14\\b=15\end{array}";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      Assert.Equal(@"\begin{array}{l}a=14\\ b=15\end{array}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestDoubleColumnArray() {
      var input = @"\begin{array}{lr}x^2&\:x<0\\x^3&\:x\geq0\end{array}";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      Assert.Equal(@"\begin{array}{lr}x^2&\: x<0\\ x^3&\: x\geq 0\end{array}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestListToString_integral_a_to_b() {
      var input = @"\int_a^b";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      Assert.Equal(@"\int _a^b", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestListToString_integral() {
      var input = @"\int_wdf=\int_{\partial w}f";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Equal(@"\int _wdf=\int _{\partial w}f", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestMatrixListToString() {
      var input = @"\begin{ vmatrix}\sin(x) &\cos(x)\\-\cos(x) &\sin(x)\end{ vmatrix}= 1";
      var list = LaTeXBuilder.MathListFromLaTeX(input);

      Assert.Equal(@"\left| \begin{matrix}\sin (x)&\cos (x)\\ -\cos (x)&\sin (x)\end{matrix}\right| =1", LaTeXBuilder.MathListToLaTeX(list));
    }

  }
}
