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
      var list = LaTeXBuilder.MathListFromLaTeX(@"\frac1\frac23");
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

    [
      Theory,
      InlineData(@"1 \over c", @"\frac{1}{c}", true),
      InlineData(@"1 \atop c", @"{1 \atop c}", false)
    ]
    public void TestOverAndAtop(string input, string output, bool hasRule) {
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Collection(list,
        CheckAtom<Fraction>("", fraction => {
          Assert.Equal(hasRule, fraction.HasRule);
          Assert.Null(fraction.LeftDelimiter);
          Assert.Null(fraction.RightDelimiter);
          Assert.Collection(fraction.Numerator, CheckAtom<Number>("1"));
          Assert.Collection(fraction.Denominator, CheckAtom<Variable>("c"));
        })
      );
      Assert.Equal(output, LaTeXBuilder.MathListToLaTeX(list));
    }

    [
      Theory,
      InlineData(@"5 + {1 \over c} + 8", @"5+\frac{1}{c}+8", true),
      InlineData(@"5 + {1 \atop c} + 8", @"5+{1 \atop c}+8", false)
    ]
    public void TestOverAndAtopInParens(string input, string output, bool hasRule) {
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Collection(list,
        CheckAtom<Number>("5"),
        CheckAtom<BinaryOperator>("+"),
        CheckAtom<Fraction>("", fraction => {
          Assert.Equal(hasRule, fraction.HasRule);
          Assert.Null(fraction.LeftDelimiter);
          Assert.Null(fraction.RightDelimiter);
          Assert.Collection(fraction.Numerator, CheckAtom<Number>("1"));
          Assert.Collection(fraction.Denominator, CheckAtom<Variable>("c"));
        }),
        CheckAtom<BinaryOperator>("+"),
        CheckAtom<Number>("8")
      );
      Assert.Equal(output, LaTeXBuilder.MathListToLaTeX(list));
    }

    [
      Theory,
      InlineData(@"n \choose k", @"{n \choose k}", "(", ")"),
      InlineData(@"n \brack k", @"{n \brack k}", "[", "]"),
      InlineData(@"n \brace k", @"{n \brace k}", "{", "}"),
      InlineData(@"\binom{n}{k}", @"{n \choose k}", "(", ")"),
    ]
    public void TestChooseBrackBraceBinomial(string input, string output, string left, string right) {
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Collection(list,
        CheckAtom<Fraction>("", fraction => {
          Assert.False(fraction.HasRule);
          Assert.Equal(left, fraction.LeftDelimiter);
          Assert.Equal(right, fraction.RightDelimiter);
          Assert.Collection(fraction.Numerator, CheckAtom<Variable>("n"));
          Assert.Collection(fraction.Denominator, CheckAtom<Variable>("k"));
        })
      );
      Assert.Equal(output, LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestOverline() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\overline 2");
      Assert.Collection(list,
        CheckAtom<Overline>("", overline =>
          Assert.Collection(overline.InnerList, CheckAtom<Number>("2"))
        )
      );
      Assert.Equal(@"\overline{2}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestUnderline() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\underline 2");
      Assert.Collection(list,
        CheckAtom<Underline>("", underline =>
          Assert.Collection(underline.InnerList, CheckAtom<Number>("2"))
        )
      );
      Assert.Equal(@"\underline{2}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestAccent() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\bar x");
      Assert.Collection(list,
        CheckAtom<Accent>("\u0304", accent =>
          Assert.Collection(accent.InnerList, CheckAtom<Variable>("x"))
        )
      );
      Assert.Equal(@"\bar{x}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestMathSpace() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\!");
      Assert.Collection(list,
        CheckAtom<Space>("", space => {
          Assert.Equal(-3, space.Length);
          Assert.True(space.IsMu);
        })
      );
      Assert.Equal(@"\! ", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestMathStyle() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\textstyle y \scriptstyle x");
      Assert.Collection(list,
        CheckAtom<Style>("", style => Assert.Equal(LineStyle.Text, style.LineStyle)),
        CheckAtom<Variable>("y"),
        CheckAtom<Style>("", style2 => Assert.Equal(LineStyle.Script, style2.LineStyle)),
        CheckAtom<Variable>("x")
      );
      Assert.Equal(@"\textstyle y\scriptstyle x", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Theory]
    [InlineData("matrix", null, null)]
    [InlineData("pmatrix", "(", ")")]
    [InlineData("bmatrix", "[", "]")]
    [InlineData("Bmatrix", "{", "}")]
    public void TestMatrix(string env, string left, string right) {
      var list = LaTeXBuilder.MathListFromLaTeX($@"\begin{{{env}}} x & y \\ z & w \end{{{env}}}");
      Table table;
      if (left is null && right is null)
        table = Assert.IsType<Table>(Assert.Single(list));
      else {
        var inner = Assert.IsType<Inner>(Assert.Single(list));
        Assert.Equal(left, inner.LeftBoundary?.Nucleus);
        Assert.Equal(right, inner.RightBoundary?.Nucleus);
        table = Assert.IsType<Table>(Assert.Single(inner.InnerList));
      }
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
          Assert.Collection(table.Cells[j][i],
            CheckAtom<Style>("", style => Assert.Equal(LineStyle.Text, style.LineStyle)),
            atom => Assert.IsType<Variable>(atom)
          );
        }
      }
      var l = left != null ? $@"\left{left} " : left;
      var r = right != null ? $@"\right{right} " : right;
      Assert.Equal($@"{l}\begin{{{env}}}x&y\\ z&w\end{{{env}}}{r}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestDefaultTable() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"x \\ y");
      var table = Assert.IsType<Table>(Assert.Single(list));
      CheckAtom<Table>("")(table);
      Assert.Null(table.Environment);
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(0, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(1, table.NColumns);
      for (int i = 0; i < 1; i++) {
        Assert.Equal(ColumnAlignment.Left, table.GetAlignment(i));
        for (int j = 0; j < 2; j++) {
          Assert.IsType<Variable>(Assert.Single(table.Cells[j][i]));
        }
      }
      Assert.Equal(@"x\\ y", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestTableWithColumns() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"x & y \\ z & w");
      var table = Assert.IsType<Table>(Assert.Single(list));
      CheckAtom<Table>("")(table);
      Assert.Null(table.Environment);
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(0, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(2, table.NColumns);
      for (int i = 0; i < 2; i++) {
        Assert.Equal(ColumnAlignment.Left, table.GetAlignment(i));
        for (int j = 0; j < 2; j++) {
          Assert.IsType<Variable>(Assert.Single(table.Cells[j][i]));
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
      var table = Assert.IsType<Table>(Assert.Single(list));
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
            Assert.IsType<Variable>(Assert.Single(cell));
          } else {
            Assert.Collection(cell,
              cell0 => Assert.IsType<Ordinary>(cell0),
              cell1 => Assert.IsType<Variable>(cell1)
            );
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
      var table = Assert.IsType<Table>(Assert.Single(list));
      CheckAtom<Table>("")(table);
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(0, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(1, table.NColumns);
      Assert.Equal(ColumnAlignment.Center, Assert.Single(table.Alignments));
      for (int j = 0; j < 2; j++) {
        Assert.IsType<Variable>(Assert.Single(table.Cells[j][0]));
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
    [InlineData(@"\limits")]
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
      Assert.Collection(list2,
        CheckAtom<LargeOperator>("lcm"),
        CheckAtom<Open>("("),
        CheckAtom<Variable>("a"),
        CheckAtom<Punctuation>(","),
        CheckAtom<Variable>("b"),
        CheckAtom<Close>(")")
      );
      Assert.Equal(@"\lcm (a,b)", LaTeXBuilder.MathListToLaTeX(list2));
    }

    [Fact]
    public void TestFontSingle() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\mathbf x");
      Assert.Collection(list, CheckAtom<Variable>("x",
        variable => Assert.Equal(FontStyle.Bold, variable.FontStyle)));
      Assert.Equal(@"\mathbf{x}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestFontMultipleCharacters() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\frak{xy}");
      Assert.Collection(list,
        CheckAtom<Variable>("x", variable => Assert.Equal(FontStyle.Fraktur, variable.FontStyle)),
        CheckAtom<Variable>("y", variable => Assert.Equal(FontStyle.Fraktur, variable.FontStyle))
      );
      Assert.Equal(@"\mathfrak{xy}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestFontOneCharacterInside() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\sqrt \mathrm x y");
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
      var list = LaTeXBuilder.MathListFromLaTeX(@"\text{x y}");
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
      var list = LaTeXBuilder.MathListFromLaTeX(@"\int");

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
      var list = LaTeXBuilder.MathListFromLaTeX(@"\sum");
      Assert.Single(list);
      var op = list[0] as LargeOperator;
      Assert.IsType<LargeOperator>(op);
      Assert.Null(op.Limits);
      Assert.Equal(@"\sum ", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestNoLimits() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\sum\nolimits");
      Assert.Single(list);
      var op = list[0] as LargeOperator;
      Assert.IsType<LargeOperator>(op);
      Assert.False(op.Limits);
      Assert.Equal(@"\sum \nolimits ", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestColor() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\color{#F00}a");
      Assert.Single(list);
      var op = list[0] as Color;
      Assert.IsType<Color>(op);
      Assert.False(op.ScriptsAllowed);
      Assert.Equal(@"\color{#F00}{a}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestSingleColumnArray() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\begin{array}{l}a=14\\b=15\end{array}");

      Assert.Single(list);
      Assert.Equal(@"\begin{array}{l}a=14\\ b=15\end{array}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestDoubleColumnArray() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\begin{array}{lr}x^2&\:x<0\\x^3&\:x\geq0\end{array}");

      Assert.Single(list);
      Assert.Equal(@"\begin{array}{lr}x^2&\: x<0\\ x^3&\: x\geq 0\end{array}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestListToString_integral_a_to_b() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\int_a^b");

      Assert.Single(list);
      Assert.Equal(@"\int _a^b", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestListToString_integral() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\int_wdf=\int_{\partial w}f");

      Assert.Equal(@"\int _wdf=\int _{\partial w}f", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestMatrixListToString() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\begin{ vmatrix}\sin(x) &\cos(x)\\-\cos(x) &\sin(x)\end{ vmatrix}= 1");

      Assert.Equal(@"\left| \begin{matrix}\sin (x)&\cos (x)\\ -\cos (x)&\sin (x)\end{matrix}\right| =1", LaTeXBuilder.MathListToLaTeX(list));
    }

  }
}
