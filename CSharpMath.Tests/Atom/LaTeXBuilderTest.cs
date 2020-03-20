using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using CSharpMath.Atom;
using CSharpMath.Atom.Atoms;

namespace CSharpMath.Tests.Atom {

  public class LaTeXBuilderTest {
    [Theory]
    [InlineData("x", new[] { typeof(Variable) }, "x")]
    [InlineData("1", new[] { typeof(Number) }, "1")]
    [InlineData("*", new[] { typeof(BinaryOperator) }, "*")]
    [InlineData("+", new[] { typeof(BinaryOperator) }, "+")]
    [InlineData(".", new[] { typeof(Number) }, ".")]
    [InlineData("(", new[] { typeof(Open) }, "(")]
    [InlineData(")", new[] { typeof(Close) }, ")")]
    [InlineData(",", new[] { typeof(Punctuation) }, ",")]
    [InlineData("?!", new[] { typeof(Close), typeof(Close) }, "?!")]
    [InlineData("=", new[] { typeof(Relation) }, "=")]
    [InlineData("x+2", new[] { typeof(Variable), typeof(BinaryOperator), typeof(Number) }, "x+2")]
    [InlineData("(2.3 * 8)", new[] { typeof(Open), typeof(Number), typeof(Number), typeof(Number), typeof(BinaryOperator), typeof(Number), typeof(Close) }, "(2.3*8)")]
    [InlineData("5{3+4}", new[] { typeof(Number), typeof(Number), typeof(BinaryOperator), typeof(Number) }, "53+4")] // braces are just for grouping
    // commands
    [InlineData(@"\pi+\theta\geq 3", new[] { typeof(Variable), typeof(BinaryOperator), typeof(Variable), typeof(Relation), typeof(Number) }, @"\pi +\theta \geq 3")]
    // aliases
    [InlineData(@"\pi\ne 5 \land 3", new[] { typeof(Variable), typeof(Relation), typeof(Number), typeof(BinaryOperator), typeof(Number) }, @"\pi \neq 5\wedge 3")]
    // control space
    [InlineData(@"x \ y", new[] { typeof(Variable), typeof(Ordinary), typeof(Variable) }, @"x\  y")]
    // spacing
    [InlineData(@"x \quad y \; z \! q", new[] { typeof(Variable), typeof(Space), typeof(Variable), typeof(Space), typeof(Variable), typeof(Space), typeof(Variable) }, @"x\quad y\; z\! q")]
    public void TestBuilder(string input, Type[] atomTypes, string output) {
      var builder = new LaTeXBuilder(input);
      var list = builder.Build();
      Assert.Null(builder.Error);

      CheckAtomTypes(list, atomTypes);

      Assert.Equal(output, LaTeXBuilder.MathListToLaTeX(list));
    }

    [Theory]
    [InlineData("x^2", "x^2", new[] { typeof(Variable) }, new[] { typeof(Number) })]
    [InlineData("x^23", "x^23", new[] { typeof(Variable), typeof(Number) }, new[] { typeof(Number) })]
    [InlineData("x^{23}", "x^{23}", new[] { typeof(Variable) }, new[] { typeof(Number), typeof(Number) })]
    [InlineData("x^2^3", "x^2{}^3", new[] { typeof(Variable), typeof(Ordinary) }, new[] { typeof(Number) })]
    [InlineData("x^{2^3}", "x^{2^3}", new[] { typeof(Variable) }, new[] { typeof(Number) }, new[] { typeof(Number) })]
    [InlineData("x^{^2*}", "x^{{}^2*}", new[] { typeof(Variable) }, new[] { typeof(Ordinary), typeof(BinaryOperator) }, new[] { typeof(Number) })]
    [InlineData("^2", "{}^2", new[] { typeof(Ordinary) }, new[] { typeof(Number) })]
    [InlineData("{}^2", "{}^2", new[] { typeof(Ordinary) }, new[] { typeof(Number) })]
    [InlineData("x^^2", "x^{}{}^2", new[] { typeof(Variable), typeof(Ordinary) }, new Type[] { })]
    [InlineData("5{x}^2", "5x^2", new[] { typeof(Number), typeof(Variable) }, new Type[] { })]
    public void TestScript(string input, string output, params Type[][] atomTypes) {
      RunScriptTest(input, atom => atom.Superscript, atomTypes, output);
      RunScriptTest(input.Replace('^', '_'), atom => atom.Subscript, atomTypes, output.Replace('^', '_'));

      void RunScriptTest
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

        Assert.Equal(output, LaTeXBuilder.MathListToLaTeX(list));
      }
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
      Assert.Equal(@"5\times 3^{2\div 2}", LaTeXBuilder.MathListToLaTeX(list));
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

    [Theory]
    [InlineData(@"1 \over c", @"\frac{1}{c}", true)]
    [InlineData(@"1 \atop c", @"{1 \atop c}", false)]
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

    [Theory]
    [InlineData(@"5 + {1 \over c} + 8", @"5+\frac{1}{c}+8", true)]
    [InlineData(@"5 + {1 \atop c} + 8", @"5+{1 \atop c}+8", false)]
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

    [Theory]
    [InlineData(@"n \choose k", @"{n \choose k}", "(", ")")]
    [InlineData(@"n \brack k", @"{n \brack k}", "[", "]")]
    [InlineData(@"n \brace k", @"{n \brace k}", "{", "}")]
    [InlineData(@"\binom{n}{k}", @"{n \choose k}", "(", ")")]
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
      var list = LaTeXBuilder.MathListFromLaTeX(@"\!\,\:\>\;\mskip15mu\quad\mkern36mu\qquad");
      Assert.Collection(list,
        CheckAtom<Space>("", space => {
          Assert.Equal(-3, space.Length);
          Assert.True(space.IsMu);
        }),
        CheckAtom<Space>("", space => {
          Assert.Equal(3, space.Length);
          Assert.True(space.IsMu);
        }),
        CheckAtom<Space>("", space => {
          Assert.Equal(4, space.Length);
          Assert.True(space.IsMu);
        }),
        CheckAtom<Space>("", space => {
          Assert.Equal(4, space.Length);
          Assert.True(space.IsMu);
        }),
        CheckAtom<Space>("", space => {
          Assert.Equal(5, space.Length);
          Assert.True(space.IsMu);
        }),
        CheckAtom<Space>("", space => {
          Assert.Equal(15, space.Length);
          Assert.True(space.IsMu);
        }),
        CheckAtom<Space>("", space => {
          Assert.Equal(18, space.Length);
          Assert.True(space.IsMu);
        }),
        CheckAtom<Space>("", space => {
          Assert.Equal(36, space.Length);
          Assert.True(space.IsMu);
        }),
        CheckAtom<Space>("", space => {
          Assert.Equal(36, space.Length);
          Assert.True(space.IsMu);
        })
      );
      Assert.Equal(@"\! \, \: \: \; \mkern15.0mu\quad \qquad \qquad ", LaTeXBuilder.MathListToLaTeX(list));
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
    [InlineData("matrix", null, null, null, null)]
    [InlineData("pmatrix", "(", ")", @"\left( ", @"\right) ")]
    [InlineData("bmatrix", "[", "]", @"\left[ ", @"\right] ")]
    [InlineData("Bmatrix", "{", "}", @"\left\{ ", @"\right\} ")]
    [InlineData("vmatrix", "|", "|", @"\left| ", @"\right| ")]
    [InlineData("Vmatrix", "‖", "‖", @"\left\| ", @"\right\| ")]
    public void TestMatrix(string env, string left, string right, string leftOutput, string rightOutput) {
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

      for (int col = 0; col < 2; col++) {
        Assert.Equal(ColumnAlignment.Center, table.GetAlignment(col));
        for (int row = 0; row < 2; row++) {
          Assert.Collection(table.Cells[row][col],
            CheckAtom<Style>("", style => Assert.Equal(LineStyle.Text, style.LineStyle)),
            atom => Assert.IsType<Variable>(atom)
          );
        }
      }
      Assert.Equal($@"{leftOutput}\begin{{matrix}}x&y\\ z&w\end{{matrix}}{rightOutput}",
        LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestDeterminant() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\begin{ vmatrix}\sin(x) &\cos(x)\\-\cos(x) &\sin(x)\end{ vmatrix}= 1");
      Assert.Collection(list,
        CheckAtom<Inner>("", inner =>
          Assert.Collection(inner.InnerList,
            CheckAtom<Table>("", table => {
              Assert.Equal(0, table.InterRowAdditionalSpacing);
              Assert.Equal(18, table.InterColumnSpacing);
              Assert.Equal(2, table.NRows);
              Assert.Equal(2, table.NColumns);
              for (int i = 0; i < 2 * 2; i++) {
                Assert.Equal(ColumnAlignment.Center, table.GetAlignment(i % 2));
                IEnumerable<Action<MathAtom>> Checkers() {
                  yield return
                    CheckAtom<Style>("", style => Assert.Equal(LineStyle.Text, style.LineStyle));
                  if (i == 2) yield return CheckAtom<BinaryOperator>("\u2212");
                  yield return CheckAtom<LargeOperator>(i switch { 0 => "sin", 3 => "sin", _ => "cos" });
                  yield return CheckAtom<Open>("(");
                  yield return CheckAtom<Variable>("x");
                  yield return CheckAtom<Close>(")");
                };
                Assert.Collection(table.Cells[i / 2][i % 2], Checkers().ToArray());
              }
            })
          )
        ),
        CheckAtom<Relation>("="),
        CheckAtom<Number>("1")
      );
      Assert.Equal(@"\left| \begin{matrix}\sin (x)&\cos (x)\\ -\cos (x)&\sin (x)\end{matrix}\right| =1", LaTeXBuilder.MathListToLaTeX(list));
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
      for (int col = 0; col < 1; col++) {
        Assert.Equal(ColumnAlignment.Left, table.GetAlignment(col));
        for (int row = 0; row < 2; row++) {
          Assert.IsType<Variable>(Assert.Single(table.Cells[row][col]));
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
      for (int col = 0; col < 2; col++) {
        Assert.Equal(ColumnAlignment.Left, table.GetAlignment(col));
        for (int row = 0; row < 2; row++) {
          Assert.IsType<Variable>(Assert.Single(table.Cells[row][col]));
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
      for (int col = 0; col < 2; col++) {
        var alignment = table.GetAlignment(col);
        Assert.Equal(col == 0 ? ColumnAlignment.Right : ColumnAlignment.Left, alignment);
        for (int row = 0; row < 2; row++) {
          var cell = table.Cells[row][col];
          if (col == 0) {
            Assert.IsType<Variable>(Assert.Single(cell));
          } else {
            Assert.Collection(cell,
              cell0 => CheckAtom<Ordinary>(""), // spacer
              cell1 => Assert.IsType<Variable>(cell1)
            );
          }
        }
      }
      Assert.Equal(input, LaTeXBuilder.MathListToLaTeX(list));
    }

    [Theory]
    [InlineData(@"\begin{array}{c}x\\ y\end{array}", 18)]
    [InlineData(@"\begin{displaylines}x\\ y\end{displaylines}", 0)]
    [InlineData(@"\begin{gather}x\\ y\end{gather}", 0)]
    public void TestDisplayLines(string input, float columnSpacing) {
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      var table = Assert.IsType<Table>(Assert.Single(list));
      CheckAtom<Table>("")(table);
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(columnSpacing, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(1, table.NColumns);
      Assert.Equal(ColumnAlignment.Center, Assert.Single(table.Alignments));
      for (int row = 0; row < 2; row++) {
        Assert.IsType<Variable>(Assert.Single(Assert.Single(table.Cells[row])));
      }
      Assert.Equal(input, LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestSingleColumnArray() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\begin{array}{l}a=14\\b=15\end{array}");
      Assert.Collection(list,
        CheckAtom<Table>("", table => {
          Assert.Collection(table.Alignments, a => Assert.Equal(ColumnAlignment.Left, a));
          Assert.Equal(2, table.NRows);
          Assert.Equal(1, table.NColumns);
          Assert.All(table.Cells, row =>
            Assert.Collection(row, cell =>
              Assert.Collection(cell,
                cell0 => Assert.IsType<Variable>(cell0),
                CheckAtom<Relation>("="),
                CheckAtom<Number>("1"),
                cell3 => Assert.IsType<Number>(cell3)
              )
            )
          );
        })
      );
      Assert.Equal(@"\begin{array}{l}a=14\\ b=15\end{array}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestDoubleColumnArray() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\begin{array}{lr}x^2&\:x<0\\x^3&\:x\geq0\end{array}");
      Assert.Collection(list,
        CheckAtom<Table>("", table => {
          Assert.Collection(table.Alignments,
            a => Assert.Equal(ColumnAlignment.Left, a),
            a => Assert.Equal(ColumnAlignment.Right, a)
          );
          Assert.Equal(2, table.NRows);
          Assert.Equal(2, table.NColumns);
          Assert.All(table.Cells, row =>
            Assert.Collection(row, column0 =>
              Assert.Collection(column0,
                CheckAtom<Variable>("x", var =>
                  Assert.IsType<Number>(Assert.Single(var.Superscript))
                )
              ), column1 =>
              Assert.Collection(column1,
                CheckAtom<Space>("", space => {
                  Assert.Equal(4, space.Length);
                  Assert.True(space.IsMu);
                }),
                CheckAtom<Variable>("x"),
                cell2 => Assert.IsType<Relation>(cell2),
                cell3 => Assert.IsType<Number>(cell3)
              )
            )
          );
        })
      );
      Assert.Equal(@"\begin{array}{lr}x^2&\: x<0\\ x^3&\: x\geq 0\end{array}", LaTeXBuilder.MathListToLaTeX(list));
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
    [InlineData(@"\color{notacolor}x")]
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

      LaTeXDefaults.Commands.Add("lcm", new LargeOperator("lcm", false));
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
      Assert.Collection(list,
        CheckAtom<Radical>("", radical =>
          Assert.Collection(radical.Radicand,
            CheckAtom<Variable>("x", variable => Assert.Equal(FontStyle.Roman, variable.FontStyle))
          )
        ),
        CheckAtom<Variable>("y", variable => Assert.Equal(FontStyle.Default, variable.FontStyle))
      );
      Assert.Equal(@"\sqrt{\mathrm{x}}y", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact] // This is for https://github.com/verybadcat/CSharpMath/issues/59
    public void TestFontInsideScript() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\mathbf{Gap}^2");
      Assert.Collection(list,
        CheckAtom<Variable>("G", G => Assert.Equal(FontStyle.Bold, G.FontStyle)),
        CheckAtom<Variable>("a", a => Assert.Equal(FontStyle.Bold, a.FontStyle)),
        CheckAtom<Variable>("p", p => {
          Assert.Equal(FontStyle.Bold, p.FontStyle);
          Assert.Collection(p.Superscript,
            CheckAtom<Number>("2", two => Assert.Equal(FontStyle.Default, two.FontStyle))
          );
        })
      );
      Assert.Equal(@"\mathbf{Gap^{\mathnormal{2}}}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestText() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\text{x y}");
      Assert.Collection(list,
        CheckAtom<Variable>(@"x", variable => Assert.Equal(FontStyle.Roman, variable.FontStyle)),
        CheckAtom<Ordinary>(" "),
        CheckAtom<Variable>(@"y", variable => Assert.Equal(FontStyle.Roman, variable.FontStyle))
      );
      Assert.Equal(@"\mathrm{x\  y}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestScriptOrdering() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\int_a^b");
      Assert.Collection(list,
        CheckAtom<LargeOperator>("∫", op => {
          Assert.Collection(op.Superscript, CheckAtom<Variable>("b"));
          Assert.Collection(op.Subscript, CheckAtom<Variable>("a"));
        })
      );
      Assert.Equal(@"\int _a^b", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestIntegrals() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\int_wdf=\int_{\partial w}f");
      Assert.Collection(list,
        CheckAtom<LargeOperator>("∫", op => {
          Assert.Null(op.Superscript);
          Assert.Collection(op.Subscript, CheckAtom<Variable>("w"));
        }),
        CheckAtom<Variable>("d"),
        CheckAtom<Variable>("f"),
        CheckAtom<Relation>("="),
        CheckAtom<LargeOperator>("∫", op => {
          Assert.Null(op.Superscript);
          Assert.Collection(op.Subscript, CheckAtom<Ordinary>("𝜕"), CheckAtom<Variable>("w"));
        }),
        CheckAtom<Variable>("f")
      );
      Assert.Equal(@"\int _wdf=\int _{\partial w}f", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Theory]
    [InlineData(@"\int", @"\int ", false)]
    [InlineData(@"\int\limits", @"\int \limits ", true)]
    [InlineData(@"\int\nolimits", @"\int ", false)]
    public void TestLimits(string input, string output, bool? limits) {
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Collection(list, CheckAtom<LargeOperator>("∫", op => Assert.Equal(limits, op.Limits)));
      Assert.Equal(output, LaTeXBuilder.MathListToLaTeX(list));
    }

    [Theory]
    [InlineData(@"\sum", @"\sum ", null)]
    [InlineData(@"\sum\limits", @"\sum \limits ", true)]
    [InlineData(@"\sum\nolimits", @"\sum \nolimits ", false)]
    public void TestUnspecifiedLimits(string input, string output, bool? limits) {
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Collection(list, CheckAtom<LargeOperator>("∑", op => Assert.Equal(limits, op.Limits)));
      Assert.Equal(output, LaTeXBuilder.MathListToLaTeX(list));
    }

    [Theory]
    [InlineData(@"\sin", @"\sin ", false)]
    [InlineData(@"\sin\limits", @"\sin ", false)]
    [InlineData(@"\sin\nolimits", @"\sin ", false)]
    public void TestNoLimits(string input, string output, bool? limits) {
      var list = LaTeXBuilder.MathListFromLaTeX(input);
      Assert.Collection(list, CheckAtom<LargeOperator>("sin", op => Assert.Equal(limits, op.Limits)));
      Assert.Equal(output, LaTeXBuilder.MathListToLaTeX(list));
    }

    [Theory]
    [InlineData("0xFFF", "white", 0xFF, 0xFF, 0xFF)]
    [InlineData("#ff0", "yellow", 0xFF, 0xFF, 0x00)]
    [InlineData("0xf00f", "blue", 0x00, 0x00, 0xFF)]
    [InlineData("#F0F0", "lime", 0x00, 0xFF, 0x00)]
    [InlineData("0x008000", "green", 0x00, 0x80, 0x00)]
    [InlineData("#d3D3d3", "lightgray", 0xD3, 0xD3, 0xD3)]
    [InlineData("0xFf000000", "black", 0x00, 0x00, 0x00)]
    [InlineData("#fFa9A9a9", "gray", 0xA9, 0xA9, 0xA9)]
    [InlineData("cyan", "cyan", 0x00, 0xFF, 0xFF)]
    [InlineData("BROWN", "brown", 0x96, 0x4B, 0x00)]
    [InlineData("oLIve", "olive", 0x80, 0x80, 0x00)]
    [InlineData("0x12345678", "#12345678", 0x34, 0x56, 0x78, 0x12)]
    [InlineData("#fedcba98", "#FEDCBA98", 0xDC, 0xBA, 0x98, 0xFE)]
    public void TestColor(string inColor, string outColor, byte r, byte g, byte b, byte a = 0xFF) {
      var list = LaTeXBuilder.MathListFromLaTeX($@"\color{{{inColor}}}a");
      Assert.Collection(list,
        CheckAtom<Color>("", color => {
          Assert.Equal(r, color.Colour.R);
          Assert.Equal(g, color.Colour.G);
          Assert.Equal(b, color.Colour.B);
          Assert.Equal(a, color.Colour.A);
          Assert.False(color.ScriptsAllowed);
        })
      );
      Assert.Equal($@"\color{{{outColor}}}{{a}}", LaTeXBuilder.MathListToLaTeX(list));
    }

    [Fact]
    public void TestColorScripts() {
      var list = LaTeXBuilder.MathListFromLaTeX(@"\color{red}_2");
      Assert.Collection(list,
        CheckAtom<Color>("", color => {
          Assert.Equal("red", color.Colour.ToString());
          Assert.Null(color.Subscript);
        }),
        CheckAtom<Ordinary>("", ord =>
          Assert.Collection(ord.Subscript, CheckAtom<Number>("2"))
        )
      );
      Assert.Equal(@"\color{red}{}{}_2", LaTeXBuilder.MathListToLaTeX(list));
    }
  }
}
