using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using CSharpMath.Atom;
using CSharpMath.Atom.Atoms;
using System.Text;

namespace CSharpMath.CoreTests {
  public class LaTeXParserTest {
    public static MathList ParseLaTeX(string latex) {
      var builder = new LaTeXParser(latex);
      if (builder.Build() is { } mathList) {
        Assert.Null(builder.Error);
        return mathList;
      } else throw new Xunit.Sdk.NotNullException();
    }

    [Theory]
    [InlineData("x", new[] { typeof(Variable) }, "x")]
    [InlineData("1", new[] { typeof(Number) }, "1")]
    [InlineData("*", new[] { typeof(BinaryOperator) }, "*")]
    [InlineData("+", new[] { typeof(BinaryOperator) }, "+")]
    [InlineData(".", new[] { typeof(Number) }, ".")]
    [InlineData("(", new[] { typeof(Open) }, "(")]
    [InlineData(")", new[] { typeof(Close) }, ")")]
    [InlineData(",", new[] { typeof(Punctuation) }, ",")]
    [InlineData("?!", new[] { typeof(Punctuation), typeof(Punctuation) }, "?!")]
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
      var list = ParseLaTeX(input);

      CheckAtomTypes(list, atomTypes);

      Assert.Equal(output, LaTeXParser.MathListToLaTeX(list).ToString());
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
    [InlineData("5{x}^2", "5x^2", new[] { typeof(Number), typeof(Variable) }, new Type[] { })]
    public void TestScript(string input, string output, params Type[][] atomTypes) {
      RunScriptTest(input, atom => atom.Superscript, atomTypes, output);
      RunScriptTest(input.Replace('^', '_'), atom => atom.Subscript, atomTypes, output.Replace('^', '_'));

      void RunScriptTest
        (string input, Func<MathAtom, MathList> scriptGetter, Type[][] atomTypes, string output) {
        var list = ParseLaTeX(input);

        var expandedList = list.Clone(false);
        CheckAtomTypes(expandedList, atomTypes[0]);

        var firstAtom = expandedList[0];
        var types = atomTypes[1];
        if (types.Length > 0)
          Assert.NotEmpty(scriptGetter(firstAtom));

        var scriptList = scriptGetter(firstAtom);
        CheckAtomTypes(scriptList, atomTypes[1]);
        if (atomTypes.Length == 3) {
          // one more level
          Assert.NotEmpty(scriptList);
          var firstScript = scriptList[0];
          var scriptScriptList = scriptGetter(firstScript);
          CheckAtomTypes(scriptScriptList, atomTypes[2]);
        }

        Assert.Equal(output, LaTeXParser.MathListToLaTeX(list).ToString());
      }
    }

    /// <summary>Safe to call with a null list. Types cannot be null however.</summary>
    private void CheckAtomTypes(MathList list, params Type[] types) {
      int atomCount = list.Atoms.Count;
      Assert.Equal(types.Length, atomCount);
      for (int i = 0; i < atomCount; i++) {
        var atom = list[i];
        Assert.NotNull(atom);
        Assert.IsType(types[i], atom);
      }
    }

    private Action<MathAtom> CheckAtom<T>
      (string nucleus, Action<T>? action = null) where T : MathAtom =>
      atom => {
        var actualAtom = Assert.IsType<T>(atom);
        Assert.Equal(nucleus, actualAtom.Nucleus);
        action?.Invoke(actualAtom);
      };

    [Fact]
    public void TestSymbols() {
      var list = ParseLaTeX(@"5\times3^{2\div2}");
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
      Assert.Equal(@"5\times 3^{2\div 2}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestFraction() {
      var list = ParseLaTeX(@"\frac1c");
      Assert.Collection(list,
        CheckAtom<Fraction>("", fraction => {
          Assert.True(fraction.HasRule);
          Assert.Null(fraction.LeftDelimiter);
          Assert.Null(fraction.RightDelimiter);
          Assert.Collection(fraction.Numerator, CheckAtom<Number>("1"));
          Assert.Collection(fraction.Denominator, CheckAtom<Variable>("c"));
        })
      );
      Assert.Equal(@"\frac{1}{c}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestFractionInFraction() {
      var list = ParseLaTeX(@"\frac1\frac23");
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
      Assert.Equal(@"\frac{1}{\frac{2}{3}}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestSqrt() {
      var list = ParseLaTeX(@"\sqrt2");
      Assert.Collection(list,
        CheckAtom<Radical>("", radical => {
          Assert.Empty(radical.Degree);
          Assert.Collection(radical.Radicand, CheckAtom<Number>("2"));
        })
      );
      Assert.Equal(@"\sqrt{2}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestSqrtInSqrt() {
      var list = ParseLaTeX(@"\sqrt\sqrt2");
      Assert.Collection(list,
        CheckAtom<Radical>("", radical =>
          Assert.Collection(radical.Radicand,
            CheckAtom<Radical>("", subRadical =>
              Assert.Collection(subRadical.Radicand, CheckAtom<Number>("2"))
            )
          )
        )
      );
      Assert.Equal(@"\sqrt{\sqrt{2}}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestRadical() {
      var list = ParseLaTeX(@"\sqrt[3]2");
      Assert.Collection(list,
        CheckAtom<Radical>("", radical => {
          Assert.Collection(radical.Degree, CheckAtom<Number>("3"));
          Assert.Collection(radical.Radicand, CheckAtom<Number>("2"));
        })
      );
      Assert.Equal(@"\sqrt[3]{2}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestBra() {
      var list = ParseLaTeX(@"\Bra{i}");
      Assert.Collection(list,
        CheckAtom<Inner>("", inner => {
          Assert.Equal("〈", inner.LeftBoundary.Nucleus);
          Assert.Equal("|", inner.RightBoundary.Nucleus);
          Assert.Collection(inner.InnerList, CheckAtom<Variable>("i"));
        })
      );
      Assert.Equal(@"\Bra{i}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestKet() {
      var list = ParseLaTeX(@"\Ket{i}");
      Assert.Collection(list,
        CheckAtom<Inner>("", inner => {
          Assert.Equal("|", inner.LeftBoundary.Nucleus);
          Assert.Equal("〉", inner.RightBoundary.Nucleus);
          Assert.Collection(inner.InnerList, CheckAtom<Variable>("i"));
        })
      );
      Assert.Equal(@"\Ket{i}", LaTeXParser.MathListToLaTeX(list).ToString());
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
      var list = ParseLaTeX(input);

      CheckAtomTypes(list, expectedOutputTypes);
      Assert.Single(expectedOutputTypes, t => t == typeof(Inner));
      CheckAtom<Inner>("", inner => {
        CheckAtomTypes(inner.InnerList, expectedInnerTypes);
        Assert.Equal(leftBoundary, inner.LeftBoundary.Nucleus);
        Assert.Equal(rightBoundary, inner.RightBoundary.Nucleus);
      })(list[Array.IndexOf(expectedOutputTypes, typeof(Inner))]);
      Assert.Equal(expectedLatex, LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Theory]
    [InlineData(@"1 \over c", @"\frac{1}{c}", true)]
    [InlineData(@"1 \atop c", @"{1 \atop c}", false)]
    public void TestOverAndAtop(string input, string output, bool hasRule) {
      var list = ParseLaTeX(input);
      Assert.Collection(list,
        CheckAtom<Fraction>("", fraction => {
          Assert.Equal(hasRule, fraction.HasRule);
          Assert.Null(fraction.LeftDelimiter);
          Assert.Null(fraction.RightDelimiter);
          Assert.Collection(fraction.Numerator, CheckAtom<Number>("1"));
          Assert.Collection(fraction.Denominator, CheckAtom<Variable>("c"));
        })
      );
      Assert.Equal(output, LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Theory]
    [InlineData(@"5 + {1 \over c} + 8", @"5+\frac{1}{c}+8", true)]
    [InlineData(@"5 + {1 \atop c} + 8", @"5+{1 \atop c}+8", false)]
    public void TestOverAndAtopInParens(string input, string output, bool hasRule) {
      var list = ParseLaTeX(input);
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
      Assert.Equal(output, LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Theory]
    [InlineData(@"n \choose k", @"{n \choose k}", "(", ")")]
    [InlineData(@"n \brack k", @"{n \brack k}", "[", "]")]
    [InlineData(@"n \brace k", @"{n \brace k}", "{", "}")]
    [InlineData(@"\binom{n}{k}", @"{n \choose k}", "(", ")")]
    public void TestChooseBrackBraceBinomial(string input, string output, string left, string right) {
      var list = ParseLaTeX(input);
      Assert.Collection(list,
        CheckAtom<Fraction>("", fraction => {
          Assert.False(fraction.HasRule);
          Assert.Equal(left, fraction.LeftDelimiter);
          Assert.Equal(right, fraction.RightDelimiter);
          Assert.Collection(fraction.Numerator, CheckAtom<Variable>("n"));
          Assert.Collection(fraction.Denominator, CheckAtom<Variable>("k"));
        })
      );
      Assert.Equal(output, LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestOverline() {
      var list = ParseLaTeX(@"\overline 2");
      Assert.Collection(list,
        CheckAtom<Overline>("", overline =>
          Assert.Collection(overline.InnerList, CheckAtom<Number>("2"))
        )
      );
      Assert.Equal(@"\overline{2}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestUnderline() {
      var list = ParseLaTeX(@"\underline 2");
      Assert.Collection(list,
        CheckAtom<Underline>("", underline =>
          Assert.Collection(underline.InnerList, CheckAtom<Number>("2"))
        )
      );
      Assert.Equal(@"\underline{2}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestAccent() {
      var list = ParseLaTeX(@"\bar x");
      Assert.Collection(list,
        CheckAtom<Accent>("\u0304", accent =>
          Assert.Collection(accent.InnerList, CheckAtom<Variable>("x"))
        )
      );
      Assert.Equal(@"\bar{x}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestMathSpace() {
      var list = ParseLaTeX(@"\!\,\:\>\;\enspace\mskip15mu\quad\mkern36mu\qquad");
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
          Assert.Equal(9, space.Length);
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
      Assert.Equal(@"\! \, \: \: \; \enspace \mkern15.0mu\quad \qquad \qquad ", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestMathStyle() {
      var list = ParseLaTeX(@"\textstyle y \scriptstyle x");
      Assert.Collection(list,
        CheckAtom<Style>("", style => Assert.Equal(LineStyle.Text, style.LineStyle)),
        CheckAtom<Variable>("y"),
        CheckAtom<Style>("", style2 => Assert.Equal(LineStyle.Script, style2.LineStyle)),
        CheckAtom<Variable>("x")
      );
      Assert.Equal(@"\textstyle y\scriptstyle x", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Theory]
    [InlineData("matrix", null, null, null, null)]
    [InlineData("pmatrix", "(", ")", @"\left( ", @"\right) ")]
    [InlineData("bmatrix", "[", "]", @"\left[ ", @"\right] ")]
    [InlineData("Bmatrix", "{", "}", @"\left\{ ", @"\right\} ")]
    [InlineData("vmatrix", "|", "|", @"\left| ", @"\right| ")]
    [InlineData("Vmatrix", "‖", "‖", @"\left\| ", @"\right\| ")]
    public void TestMatrix(string env, string left, string right, string leftOutput, string rightOutput) {
      var list = ParseLaTeX($@"\begin{{{env}}} x & y \\ z & w \end{{{env}}}");
      Table table;
      if (left is null && right is null)
        table = Assert.IsType<Table>(Assert.Single(list));
      else {
        var inner = Assert.IsType<Inner>(Assert.Single(list));
        Assert.Equal(left, inner.LeftBoundary.Nucleus);
        Assert.Equal(right, inner.RightBoundary.Nucleus);
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
        LaTeXParser.MathListToLaTeX(list).ToString());
    }
    [Theory]
    [InlineData(@"\color{red}\begin{pmatrix}1&2\\3&4\end{pmatrix}")]
    [InlineData(@"\color{red}\begin{pmatrix}{1}&2\\3&{4}\end{pmatrix}")]
    [InlineData(@"\color{red}{\begin{pmatrix}1&2\\3&4\end{pmatrix}}")]
    [InlineData(@"\color{red}{{\begin{pmatrix}1&2\\3&4\end{pmatrix}}}")]
    [InlineData(@"\color{red}\left(\begin{matrix}1&2\\3&4\end{matrix}\right)")]
    [InlineData(@"\color{red}\left( \begin{matrix}1&2\\ 3&4\end{matrix}\right) ")]
    [InlineData(@"\color{red}{\left( \begin{matrix}1&2\\ 3&4\end{matrix}\right) }")]
    [InlineData(@"\color{red}{{\left( \begin{matrix}1&2\\ 3&4\end{matrix}\right) }}")]
    public void TestRedMatrix(string input) {
      var list = ParseLaTeX(input);
      Assert.Collection(list, CheckAtom<Colored>("", colored => {
        Assert.Equal(System.Drawing.Color.FromArgb(255, 0, 0), colored.Color);
        Assert.Collection(colored.InnerList,
          CheckAtom<Inner>("", inner => {
            Assert.Equal(new Boundary("("), inner.LeftBoundary);
            Assert.Equal(new Boundary(")"), inner.RightBoundary);
            Assert.Collection(inner.InnerList,
              CheckAtom<Table>("", table => {
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
                      atom => Assert.IsType<Number>(atom)
                    );
                  }
                }
              })
            );
          })
        );
      }));
      Assert.Equal(@"\color{red}{\left( \begin{matrix}1&2\\ 3&4\end{matrix}\right) }",
        LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestDeterminant() {
      var list = ParseLaTeX(@"\begin{ vmatrix}\sin(x) &\cos(x)\\-\cos(x) &\sin(x)\end{ vmatrix}= 1");
      Assert.Collection(list,
        CheckAtom<Inner>("", inner =>
          Assert.Collection(inner.InnerList,
            CheckAtom<Table>("", table => {
              Assert.Equal("matrix", table.Environment);
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
      Assert.Equal(@"\left| \begin{matrix}\sin (x)&\cos (x)\\ -\cos (x)&\sin (x)\end{matrix}\right| =1", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestDefaultEmptyTable() {
      var list = ParseLaTeX(@"\\");
      var table = Assert.IsType<Table>(Assert.Single(list));
      CheckAtom<Table>("")(table);
      Assert.Null(table.Environment);
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(0, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(1, table.NColumns);
      Assert.Equal(ColumnAlignment.Left, table.GetAlignment(0));
      Assert.Equal(ColumnAlignment.Center, table.GetAlignment(1));
      Assert.Collection(table.Cells, row0 => Assert.Empty(Assert.Single(row0)), Assert.Empty);
      Assert.Equal(@"\\ ", LaTeXParser.MathListToLaTeX(list).ToString());
    }
    [Fact]
    public void TestDefaultTable() {
      var list = ParseLaTeX(@"x \\ y");
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
      Assert.Equal(@"x\\ y", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Theory]
    [InlineData(@"\left(x\\\right)")]
    [InlineData(@"\left(x \\ \right)")]
    [InlineData(@"\left( x\\ \right)")]
    [InlineData(@"\left( x\\ \right) ")]
    [InlineData(@"\left({x\\}\right) ")]
    [InlineData(@"\left({{x\\}}\right) ")]
    public void TestDefaultTableInInner(string input) {
      var list = ParseLaTeX(input);
      CheckAtom<Inner>("", inner => {
        Assert.Equal(new Boundary("("), inner.LeftBoundary);
        Assert.Equal(new Boundary(")"), inner.RightBoundary);
        CheckAtom<Table>("", table => {
          Assert.Null(table.Environment);
          Assert.Equal(1, table.InterRowAdditionalSpacing);
          Assert.Equal(0, table.InterColumnSpacing);
          Assert.Equal(2, table.NRows);
          Assert.Equal(1, table.NColumns);
          Assert.Equal(ColumnAlignment.Left, table.GetAlignment(0));
          Assert.IsType<Variable>(Assert.Single(table.Cells[0][0]));
          Assert.Empty(table.Cells[1][0]);
        })(Assert.Single(inner.InnerList));
      })(Assert.Single(list));
      Assert.Equal(@"\left( x\\ \right) ", LaTeXParser.MathListToLaTeX(list).ToString());
    }
    [Theory]
    [InlineData(@"\left(\\\right)")]
    [InlineData(@"\left( \\ \right) ")]
    [InlineData(@"\left({\\}\right) ")]
    [InlineData(@"\left({{\\}}\right) ")]
    public void TestEmptyTableInInner(string input) {
      var list = ParseLaTeX(input);
      CheckAtom<Inner>("", inner => {
        Assert.Equal(new Boundary("("), inner.LeftBoundary);
        Assert.Equal(new Boundary(")"), inner.RightBoundary);
        CheckAtom<Table>("", table => {
          Assert.Null(table.Environment);
          Assert.Equal(1, table.InterRowAdditionalSpacing);
          Assert.Equal(0, table.InterColumnSpacing);
          Assert.Equal(2, table.NRows);
          Assert.Equal(1, table.NColumns);
          for (int col = 0; col < 1; col++) {
            Assert.Equal(ColumnAlignment.Left, table.GetAlignment(col));
            for (int row = 0; row < 2; row++) {
              Assert.Empty(table.Cells[row][col]);
            }
          }
        })(Assert.Single(inner.InnerList));
      })(Assert.Single(list));
      Assert.Equal(@"\left( \\ \right) ", LaTeXParser.MathListToLaTeX(list).ToString());
    }
    [Theory]
    [InlineData(@"x\\1\left(2\right)3\\x")]
    [InlineData(@"x\\ 1\left( 2\right) 3\\ x")]
    public void TestInnerInTable(string input) {
      var list = ParseLaTeX(input);
      CheckAtom<Table>("", table => {
        Assert.Null(table.Environment);
        Assert.Equal(1, table.InterRowAdditionalSpacing);
        Assert.Equal(0, table.InterColumnSpacing);
        Assert.Equal(3, table.NRows);
        Assert.Equal(1, table.NColumns);
        for (int col = 0; col < 1; col++) {
          Assert.Equal(ColumnAlignment.Left, table.GetAlignment(col));
          for (int row = 0; row < 3; row++) {
            if (row == 1)
              Assert.Collection(
                table.Cells[row][col],
                CheckAtom<Number>("1"),
                CheckAtom<Inner>("", inner => {
                  Assert.Equal(new Boundary("("), inner.LeftBoundary);
                  Assert.Collection(inner.InnerList, CheckAtom<Number>("2"));
                  Assert.Equal(new Boundary(")"), inner.RightBoundary);
                }),
                CheckAtom<Number>("3")
              );
            else
              Assert.Collection(
                table.Cells[row][col],
                CheckAtom<Variable>("x")
              );
          }
        }
      })(Assert.Single(list));
      Assert.Equal(@"x\\ 1\left( 2\right) 3\\ x", LaTeXParser.MathListToLaTeX(list).ToString());
    }
    [Theory]
    [InlineData(@"1\\2\left(3\begin{array}{ll}4&\left[5\\6\right]\\\left\{7\begin{cases}8\\9\end{cases}0\right\}a\end{array}b\right)c\\d", 0)]
    [InlineData(@"1\\ 2\left( 3\begin{array}{ll}4&\left[ 5\\ 6\right] \\ \left\{ 7\left\{ \, \begin{array}{l}\textstyle 8\\ \textstyle 9\end{array}\right. 0\right\} a\end{array}b\right) c\\ d", 1)]
    public void TestTablesAndInners(string input, float casesInterRowAdditionalSpacing) {
      var list = ParseLaTeX(input);
      CheckAtom<Table>("", table => {
        Assert.Null(table.Environment);
        Assert.Equal(1, table.InterRowAdditionalSpacing);
        Assert.Equal(0, table.InterColumnSpacing);
        Assert.Equal(3, table.NRows);
        Assert.Equal(1, table.NColumns);
        Assert.Equal(ColumnAlignment.Left, table.GetAlignment(0));
        Assert.Equal(ColumnAlignment.Center, table.GetAlignment(1));
        Assert.Collection(table.Cells[0][0], CheckAtom<Number>("1"));
        Assert.Collection(table.Cells[1][0],
          CheckAtom<Number>("2"),
          CheckAtom<Inner>("", inner => {
            Assert.Equal(new Boundary("("), inner.LeftBoundary);
            Assert.Equal(new Boundary(")"), inner.RightBoundary);
            Assert.Collection(inner.InnerList,
              CheckAtom<Number>("3"),
              CheckAtom<Table>("", array => {
                Assert.Equal("array", array.Environment);
                Assert.Equal(1, array.InterRowAdditionalSpacing);
                Assert.Equal(18, array.InterColumnSpacing);
                Assert.Equal(2, array.NRows);
                Assert.Equal(2, array.NColumns);
                Assert.Equal(ColumnAlignment.Left, array.GetAlignment(0));
                Assert.Equal(ColumnAlignment.Left, array.GetAlignment(1));
                Assert.Equal(ColumnAlignment.Center, array.GetAlignment(2));
                Assert.Collection(array.Cells[0][0], CheckAtom<Number>("4"));
                Assert.Collection(array.Cells[0][1], CheckAtom<Inner>("", inner56 => {
                  Assert.Equal(new Boundary("["), inner56.LeftBoundary);
                  Assert.Equal(new Boundary("]"), inner56.RightBoundary);
                  Assert.Collection(inner56.InnerList, CheckAtom<Table>("", table56 => {
                    Assert.Null(table56.Environment);
                    Assert.Equal(1, table56.InterRowAdditionalSpacing);
                    Assert.Equal(0, table56.InterColumnSpacing);
                    Assert.Equal(2, table56.NRows);
                    Assert.Equal(1, table56.NColumns);
                    Assert.Equal(ColumnAlignment.Left, table56.GetAlignment(0));
                    Assert.Equal(ColumnAlignment.Center, table56.GetAlignment(1));
                    Assert.Collection(table56.Cells[0][0], CheckAtom<Number>("5"));
                    Assert.Collection(table56.Cells[1][0], CheckAtom<Number>("6"));
                  }));
                }));
                Assert.Collection(array.Cells[1][0],
                  CheckAtom<Inner>("", innerCases => {
                    Assert.Equal(new Boundary("{"), innerCases.LeftBoundary);
                    Assert.Equal(new Boundary("}"), innerCases.RightBoundary);
                    Assert.Collection(innerCases.InnerList,
                      CheckAtom<Number>("7"),
                      CheckAtom<Inner>("", innerCasesInner => {
                        Assert.Equal(new Boundary("{"), innerCasesInner.LeftBoundary);
                        Assert.Equal(Boundary.Empty, innerCasesInner.RightBoundary);
                        Assert.Collection(innerCasesInner.InnerList,
                          CheckAtom<Space>("", space => Assert.Equal(3, space.Length)),
                          CheckAtom<Table>("", tableCases => {
                            Assert.Equal("array", tableCases.Environment);
                            Assert.Equal(casesInterRowAdditionalSpacing, tableCases.InterRowAdditionalSpacing);
                            Assert.Equal(18, tableCases.InterColumnSpacing);
                            Assert.Equal(2, tableCases.NRows);
                            Assert.Equal(1, tableCases.NColumns);
                            Assert.Equal(ColumnAlignment.Left, tableCases.GetAlignment(0));
                            Assert.Equal(ColumnAlignment.Center, tableCases.GetAlignment(1));
                            Assert.Collection(tableCases.Cells[0][0],
                              CheckAtom<Style>("", style => Assert.Equal(LineStyle.Text, style.LineStyle)),
                              CheckAtom<Number>("8"));
                            Assert.Collection(tableCases.Cells[1][0],
                              CheckAtom<Style>("", style => Assert.Equal(LineStyle.Text, style.LineStyle)),
                              CheckAtom<Number>("9"));
                          })
                        );
                      }),
                      CheckAtom<Number>("0")
                    );
                  }),
                  CheckAtom<Variable>("a")
                );
              }),
              CheckAtom<Variable>("b")
            );
          }),
          CheckAtom<Variable>("c"));
        Assert.Collection(table.Cells[2][0], CheckAtom<Variable>("d"));
      })(Assert.Single(list));
      Assert.Equal(@"1\\ 2\left( 3\begin{array}{ll}4&\left[ 5\\ 6\right] \\ \left\{ 7\left\{ \, \begin{array}{l}\textstyle 8\\ \textstyle 9\end{array}\right. 0\right\} a\end{array}b\right) c\\ d", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestTableWithColumns() {
      var list = ParseLaTeX(@"x & y \\ z & w");
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
      Assert.Equal(@"x&y\\ z&w", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Theory]
    [InlineData("eqalign")]
    [InlineData("split")]
    [InlineData("aligned")]
    public void TestEqAlign(string environment) {
      var input = $@"\begin{{{environment}}}x&y\\ z&w\end{{{environment}}}";
      var list = ParseLaTeX(input);
      var table = Assert.IsType<Table>(Assert.Single(list));
      CheckAtom<Table>("")(table);
      Assert.Equal(environment, table.Environment);
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
      Assert.Equal(input, LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Theory]
    [InlineData("array", 18)]
    [InlineData("displaylines", 0)]
    [InlineData("gather", 0)]
    public void TestDisplayLines(string environment, float columnSpacing) {
      var input = $@"\begin{{{environment}}}{(environment == "array" ? "{c}" : null)}x\\ y\end{{{environment}}}";
      var list = ParseLaTeX(input);
      var table = Assert.IsType<Table>(Assert.Single(list));
      CheckAtom<Table>("")(table);
      Assert.Equal(environment, table.Environment);
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(columnSpacing, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(1, table.NColumns);
      Assert.Equal(ColumnAlignment.Center, Assert.Single(table.Alignments));
      for (int row = 0; row < 2; row++) {
        Assert.IsType<Variable>(Assert.Single(Assert.Single(table.Cells[row])));
      }
      Assert.Equal(input, LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestSingleColumnArray() {
      var list = ParseLaTeX(@"\begin{array}{l}a=14\\b=15\end{array}");
      Assert.Collection(list,
        CheckAtom<Table>("", table => {
          Assert.Equal("array", table.Environment);
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
      Assert.Equal(@"\begin{array}{l}a=14\\ b=15\end{array}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestDoubleColumnArray() {
      var list = ParseLaTeX(@"\begin{array}{lr}x^2&\:x<0\\x^3&\:x\geq0\end{array}");
      Assert.Collection(list,
        CheckAtom<Table>("", table => {
          Assert.Equal("array", table.Environment);
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
      Assert.Equal(@"\begin{array}{lr}x^2&\: x<0\\ x^3&\: x\geq 0\end{array}", LaTeXParser.MathListToLaTeX(list).ToString());
    }
    [Fact]
    public void TestCases() {
      var list = ParseLaTeX(@"\begin{cases} y=x^2-x+3 \\ y=x^2+\sqrt x-\frac2x \end{cases}");
      Assert.Collection(list,
        CheckAtom<Inner>("", inner => {
          Assert.Equal(new Boundary("{"), inner.LeftBoundary);
          Assert.Equal(Boundary.Empty, inner.RightBoundary);
          Assert.Collection(inner.InnerList,
            CheckAtom<Space>("", space => {
              Assert.Equal(3, space.Length);
              Assert.True(space.IsMu);
            }),
            CheckAtom<Table>("", table => {
              Assert.Equal("array", table.Environment);
              Assert.Collection(table.Alignments, a => Assert.Equal(ColumnAlignment.Left, a));
              Assert.Equal(2, table.NRows);
              Assert.Equal(1, table.NColumns);
              Assert.All(table.Cells, row =>
                Assert.Collection(row, cell =>
                  Assert.Collection(cell,
                    CheckAtom<Style>("", style => Assert.Equal(LineStyle.Text, style.LineStyle)),
                    CheckAtom<Variable>("y"),
                    CheckAtom<Relation>("="),
                    CheckAtom<Variable>("x", x => Assert.Collection(x.Superscript, CheckAtom<Number>("2"))),
                    cell3 => Assert.IsType<BinaryOperator>(cell3),
                    cell4 => { },
                    cell5 => Assert.IsType<BinaryOperator>(cell5),
                    cell6 => { }
                  )
                )
              );
            })
          );
        })
      );
      Assert.Equal(@"\left\{ \, \begin{array}{l}\textstyle y=x^2-x+3\\ \textstyle y=x^2+\sqrt{x}-\frac{2}{x}\end{array}\right. ", LaTeXParser.MathListToLaTeX(list).ToString());
    }
    [Fact]
    public void TestCases2() {
      var list = ParseLaTeX(@"\begin{cases} y=x^2-x+3 &\text{for }x\leq0 \\ y=x^2+\sqrt x-\frac2x &\text{for }x>0 \end{cases}");
      Assert.Collection(list,
        CheckAtom<Inner>("", inner => {
          Assert.Equal(new Boundary("{"), inner.LeftBoundary);
          Assert.Equal(Boundary.Empty, inner.RightBoundary);
          Assert.Collection(inner.InnerList,
            CheckAtom<Space>("", space => {
              Assert.Equal(3, space.Length);
              Assert.True(space.IsMu);
            }),
            CheckAtom<Table>("", table => {
              Assert.Equal("array", table.Environment);
              Assert.Collection(table.Alignments,
                a => Assert.Equal(ColumnAlignment.Left, a),
                a => Assert.Equal(ColumnAlignment.Left, a));
              Assert.Equal(2, table.NRows);
              Assert.Equal(2, table.NColumns);
              Assert.All(table.Cells, row =>
                Assert.Collection(row, col0 =>
                  Assert.Collection(col0,
                    CheckAtom<Style>("", style => Assert.Equal(LineStyle.Text, style.LineStyle)),
                    CheckAtom<Variable>("y"),
                    CheckAtom<Relation>("="),
                    CheckAtom<Variable>("x", x => Assert.Collection(x.Superscript, CheckAtom<Number>("2"))),
                    cell3 => Assert.IsType<BinaryOperator>(cell3),
                    cell4 => { },
                    cell5 => Assert.IsType<BinaryOperator>(cell5),
                    cell6 => { }
                  ), col1 =>
                  Assert.Collection(col1,
                    CheckAtom<Style>("", style => Assert.Equal(LineStyle.Text, style.LineStyle)),
                    CheckAtom<Variable>("f", f => Assert.Equal(FontStyle.Roman, f.FontStyle)),
                    CheckAtom<Variable>("o", o => Assert.Equal(FontStyle.Roman, o.FontStyle)),
                    CheckAtom<Variable>("r", r => Assert.Equal(FontStyle.Roman, r.FontStyle)),
                    CheckAtom<Ordinary>(" ", space => Assert.Equal(FontStyle.Roman, space.FontStyle)),
                    CheckAtom<Variable>("x", x => Assert.Equal(FontStyle.Default, x.FontStyle)),
                    cell3 => Assert.IsType<Relation>(cell3),
                    CheckAtom<Number>("0")
                  )
                )
              );
            })
          );
        })
      );
      Assert.Equal(@"\left\{ \, \begin{array}{ll}\textstyle y=x^2-x+3&\textstyle \mathrm{for\  }x\leq 0\\ \textstyle y=x^2+\sqrt{x}-\frac{2}{x}&\textstyle \mathrm{for\  }x>0\end{array}\right. ", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestCustom() {
      var input = @"\lcm(a,b)";
      var builder = new LaTeXParser(input);
      var list = builder.Build();
      Assert.Null(list);
      Assert.NotNull(builder.Error);

      LaTeXSettings.Commands.Add("lcm", new LargeOperator("lcm", false));
      var list2 = ParseLaTeX(input);
      Assert.Collection(list2,
        CheckAtom<LargeOperator>("lcm"),
        CheckAtom<Open>("("),
        CheckAtom<Variable>("a"),
        CheckAtom<Punctuation>(","),
        CheckAtom<Variable>("b"),
        CheckAtom<Close>(")")
      );
      Assert.Equal(@"\lcm (a,b)", LaTeXParser.MathListToLaTeX(list2).ToString());
    }

    [Fact]
    public void TestFontSingle() {
      var list = ParseLaTeX(@"\mathbf x");
      Assert.Collection(list, CheckAtom<Variable>("x",
        variable => Assert.Equal(FontStyle.Bold, variable.FontStyle)));
      Assert.Equal(@"\mathbf{x}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestFontMultipleCharacters() {
      var list = ParseLaTeX(@"\frak{xy}");
      Assert.Collection(list,
        CheckAtom<Variable>("x", variable => Assert.Equal(FontStyle.Fraktur, variable.FontStyle)),
        CheckAtom<Variable>("y", variable => Assert.Equal(FontStyle.Fraktur, variable.FontStyle))
      );
      Assert.Equal(@"\mathfrak{xy}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestFontOneCharacterInside() {
      var list = ParseLaTeX(@"\sqrt \mathrm x y");
      Assert.Collection(list,
        CheckAtom<Radical>("", radical =>
          Assert.Collection(radical.Radicand,
            CheckAtom<Variable>("x", variable => Assert.Equal(FontStyle.Roman, variable.FontStyle))
          )
        ),
        CheckAtom<Variable>("y", variable => Assert.Equal(FontStyle.Default, variable.FontStyle))
      );
      Assert.Equal(@"\sqrt{\mathrm{x}}y", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact] // This is for https://github.com/verybadcat/CSharpMath/issues/59
    public void TestFontInsideScript() {
      var list = ParseLaTeX(@"\mathbf{Gap}^2");
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
      Assert.Equal(@"\mathbf{Gap^{\mathnormal{2}}}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestText() {
      var list = ParseLaTeX(@"\text{x y}");
      Assert.Collection(list,
        CheckAtom<Variable>(@"x", variable => Assert.Equal(FontStyle.Roman, variable.FontStyle)),
        CheckAtom<Ordinary>(" "),
        CheckAtom<Variable>(@"y", variable => Assert.Equal(FontStyle.Roman, variable.FontStyle))
      );
      Assert.Equal(@"\mathrm{x\  y}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestScriptOrdering() {
      var list = ParseLaTeX(@"\int_a^b");
      Assert.Collection(list,
        CheckAtom<LargeOperator>("∫", op => {
          Assert.Collection(op.Superscript, CheckAtom<Variable>("b"));
          Assert.Collection(op.Subscript, CheckAtom<Variable>("a"));
        })
      );
      Assert.Equal(@"\int _a^b", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestIntegrals() {
      var list = ParseLaTeX(@"\int_wdf=\int_{\partial w}f");
      Assert.Collection(list,
        CheckAtom<LargeOperator>("∫", op => {
          Assert.Empty(op.Superscript);
          Assert.Collection(op.Subscript, CheckAtom<Variable>("w"));
        }),
        CheckAtom<Variable>("d"),
        CheckAtom<Variable>("f"),
        CheckAtom<Relation>("="),
        CheckAtom<LargeOperator>("∫", op => {
          Assert.Empty(op.Superscript);
          Assert.Collection(op.Subscript, CheckAtom<Ordinary>("𝜕"), CheckAtom<Variable>("w"));
        }),
        CheckAtom<Variable>("f")
      );
      Assert.Equal(@"\int _wdf=\int _{\partial w}f", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Theory]
    [InlineData(@"\int", @"\int ", false)]
    [InlineData(@"\int\limits", @"\int \limits ", true)]
    [InlineData(@"\int\nolimits", @"\int ", false)]
    public void TestLimits(string input, string output, bool? limits) {
      var list = ParseLaTeX(input);
      Assert.Collection(list, CheckAtom<LargeOperator>("∫", op => Assert.Equal(limits, op.Limits)));
      Assert.Equal(output, LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Theory]
    [InlineData(@"\sum", @"\sum ", null)]
    [InlineData(@"\sum\limits", @"\sum \limits ", true)]
    [InlineData(@"\sum\nolimits", @"\sum \nolimits ", false)]
    public void TestUnspecifiedLimits(string input, string output, bool? limits) {
      var list = ParseLaTeX(input);
      Assert.Collection(list, CheckAtom<LargeOperator>("∑", op => Assert.Equal(limits, op.Limits)));
      Assert.Equal(output, LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Theory]
    [InlineData(@"\sin", @"\sin ", false)]
    [InlineData(@"\sin\limits", @"\sin ", false)]
    [InlineData(@"\sin\nolimits", @"\sin ", false)]
    public void TestNoLimits(string input, string output, bool? limits) {
      var list = ParseLaTeX(input);
      Assert.Collection(list, CheckAtom<LargeOperator>("sin", op => Assert.Equal(limits, op.Limits)));
      Assert.Equal(output, LaTeXParser.MathListToLaTeX(list).ToString());
    }

    // Sync with CSharpMath.Rendering.Text.Tests TextLaTeXParserTests
    [Theory]
    [InlineData("0xFFFFFF", "white", 0xFF, 0xFF, 0xFF)]
    [InlineData("#ffff00", "yellow", 0xFF, 0xFF, 0x00)]
    [InlineData("0xff0000ff", "blue", 0x00, 0x00, 0xFF)]
    [InlineData("#FF00FF00", "lime", 0x00, 0xFF, 0x00)]
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
      var list = ParseLaTeX($@"\color{{{inColor}}}ab");
      Assert.Collection(list,
        CheckAtom<Colored>("", colored => {
          Assert.Equal(r, colored.Color.R);
          Assert.Equal(g, colored.Color.G);
          Assert.Equal(b, colored.Color.B);
          Assert.Equal(a, colored.Color.A);
          Assert.False(colored.ScriptsAllowed);
          Assert.Collection(colored.InnerList, CheckAtom<Variable>("a"));
        }),
        CheckAtom<Variable>("b")
      );
      Assert.Equal($@"\color{{{outColor}}}{{a}}b", LaTeXParser.MathListToLaTeX(list).ToString());

      list = ParseLaTeX($@"\colorbox{{{inColor}}}ab");
      Assert.Collection(list,
        CheckAtom<ColorBox>("", colorBox => {
          Assert.Equal(r, colorBox.Color.R);
          Assert.Equal(g, colorBox.Color.G);
          Assert.Equal(b, colorBox.Color.B);
          Assert.Equal(a, colorBox.Color.A);
          Assert.False(colorBox.ScriptsAllowed);
          Assert.Collection(colorBox.InnerList, CheckAtom<Variable>("a"));
        }),
        CheckAtom<Variable>("b")
      );
      Assert.Equal($@"\colorbox{{{outColor}}}{{a}}b", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Fact]
    public void TestColorScripts() {
      var list = ParseLaTeX(@"\color{red}1\colorbox{blue}2");
      Assert.Collection(list,
        CheckAtom<Colored>("", colored => {
          Assert.Equal("red", Structures.ColorExtensions.ToTexString(colored.Color));
          Assert.Empty(colored.Superscript);
          Assert.Throws<InvalidOperationException>(() => colored.Superscript.Add(new Variable("a")));
          Assert.Throws<InvalidOperationException>(() => colored.Superscript.Append(new MathList(new Variable("a"))));
          Assert.Empty(colored.Subscript);
          Assert.Throws<InvalidOperationException>(() => colored.Subscript.Add(new Variable("b")));
          Assert.Throws<InvalidOperationException>(() => colored.Subscript.Append(new MathList(new Variable("b"))));
          Assert.Collection(colored.InnerList, CheckAtom<Number>("1"));
        }),
        CheckAtom<ColorBox>("", colorBox => {
          Assert.Equal("blue", Structures.ColorExtensions.ToTexString(colorBox.Color));
          Assert.Empty(colorBox.Superscript);
          Assert.Throws<InvalidOperationException>(() => colorBox.Superscript.Add(new Variable("a")));
          Assert.Throws<InvalidOperationException>(() => colorBox.Superscript.Append(new MathList(new Variable("a"))));
          Assert.Empty(colorBox.Subscript);
          Assert.Throws<InvalidOperationException>(() => colorBox.Subscript.Add(new Variable("b")));
          Assert.Throws<InvalidOperationException>(() => colorBox.Subscript.Append(new MathList(new Variable("b"))));
          Assert.Collection(colorBox.InnerList, CheckAtom<Number>("2"));
        })
      );
      Assert.Equal(@"\color{red}{1}\colorbox{blue}{2}", LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Theory]
    [InlineData("s", @"\operatorname{s} ")]
    [InlineData("sin", @"\sin ")]
    public void TestOperatorName(string operatorname, string output) {
      var list = ParseLaTeX(@$"\operatorname{{{operatorname}}}");
      Assert.Collection(list, CheckAtom<LargeOperator>(operatorname));
      Assert.Equal(output, LaTeXParser.MathListToLaTeX(list).ToString());
    }

    [Theory,
     InlineData("0", 1, @"Error: Error Message
0
↑ (pos 1)"),
     InlineData("01", 1, @"Error: Error Message
01
↑ (pos 1)"),
     InlineData("01", 2, @"Error: Error Message
01
 ↑ (pos 2)"),
     InlineData("012", 1, @"Error: Error Message
012
↑ (pos 1)"),
     InlineData("012", 2, @"Error: Error Message
012
 ↑ (pos 2)"),
     InlineData("012", 3, @"Error: Error Message
012
  ↑ (pos 3)"),
     InlineData("012345678911234567892123456789", 10, @"Error: Error Message
012345678911234567892123456789
         ↑ (pos 10)"),
     InlineData("012345678911234567892123456789", 20, @"Error: Error Message
012345678911234567892123456789
                   ↑ (pos 20)"),
     InlineData("012345678911234567892123456789", 21, @"Error: Error Message
012345678911234567892123456789
                    ↑ (pos 21)"),
     InlineData("012345678911234567892123456789", 22, @"Error: Error Message
···12345678911234567892123456789
                       ↑ (pos 22)"),
     InlineData("012345678911234567892123456789", 23, @"Error: Error Message
···2345678911234567892123456789
                       ↑ (pos 23)"),
     InlineData("0123456789112345678921234567893123456789412345678951234567896123456789", 1, @"Error: Error Message
01234567891123456789212345678931234567894···
↑ (pos 1)"),
     InlineData("0123456789112345678921234567893123456789412345678951234567896123456789", 22, @"Error: Error Message
···1234567891123456789212345678931234567894123456789512345678961···
                       ↑ (pos 22)")]
    public void TestHelpfulErrorMessage(string input, int index, string expected) {
      var actual = LaTeXParser.HelpfulErrorMessage("Error Message", input, index);
      Assert.Equal(expected.Replace("\r", null), actual);
    }

    [Theory,
      InlineData(@"x^^2", @"Error: ^ cannot appear as an argument to a command
x^^2
  ↑ (pos 3)"),
      InlineData(@"x^_2", @"Error: _ cannot appear as an argument to a command
x^_2
  ↑ (pos 3)"),
      InlineData(@"x_^2", @"Error: ^ cannot appear as an argument to a command
x_^2
  ↑ (pos 3)"),
      InlineData(@"x__2", @"Error: _ cannot appear as an argument to a command
x__2
  ↑ (pos 3)"),
      InlineData(@"x^&2", @"Error: & cannot appear as an argument to a command
x^&2
  ↑ (pos 3)"),
      InlineData(@"x^}2", @"Error: } cannot appear as an argument to a command
x^}2
  ↑ (pos 3)"),
      InlineData(@"x_&2", @"Error: & cannot appear as an argument to a command
x_&2
  ↑ (pos 3)"),
      InlineData(@"x_}2", @"Error: } cannot appear as an argument to a command
x_}2
  ↑ (pos 3)"),
      InlineData(@"\sqrt^2", @"Error: ^ cannot appear as an argument to a command
\sqrt^2
     ↑ (pos 6)"),
      InlineData(@"\sqrt_2", @"Error: _ cannot appear as an argument to a command
\sqrt_2
     ↑ (pos 6)"),
      InlineData(@"\sqrt&2", @"Error: & cannot appear as an argument to a command
\sqrt&2
     ↑ (pos 6)"),
      InlineData(@"\sqrt}2", @"Error: } cannot appear as an argument to a command
\sqrt}2
     ↑ (pos 6)"),
      InlineData(@"\notacommand", @"Error: Invalid command \notacommand
\notacommand
           ↑ (pos 12)"),
      InlineData(@"\sqrt[5+3", @"Error: Expected character not found: ]
\sqrt[5+3
        ↑ (pos 9)"),
      InlineData(@"{5+3", @"Error: Missing closing brace
{5+3
   ↑ (pos 4)"),
      InlineData(@"5+3}", @"Error: Missing opening brace
5+3}
   ↑ (pos 4)"),
      InlineData(@"5+3}12", @"Error: Missing opening brace
5+3}12
   ↑ (pos 4)"),
      InlineData(@"{1+\frac{3+2", @"Error: Missing closing brace
{1+\frac{3+2
           ↑ (pos 12)"),
      InlineData(@"1+\left", @"Error: Missing delimiter for left
1+\left
      ↑ (pos 7)"),
      InlineData(@"\left{", @"Error: Missing \right for \left with delimiter {
\left{
     ↑ (pos 6)"),
      InlineData(@"\left(\frac12\right", @"Error: Missing delimiter for right
\left(\frac12\right
                  ↑ (pos 19)"),
      InlineData(@"\left 5 + 3 \right)", @"Error: Invalid delimiter for \left: 5
\left 5 + 3 \right)
      ↑ (pos 7)"),
      InlineData(@"\left(\frac12\right + 3", @"Error: Invalid delimiter for \right: +
\left(\frac12\right + 3
                    ↑ (pos 21)"),
      InlineData(@"\left\notadelimiter 5 + 3 \right)", @"Error: Invalid delimiter for \left: notadelimiter
\left\notadelimiter 5 + 3 \right)
                  ↑ (pos 19)"),
      InlineData(@"\left(\frac12\right\notadelimiter + 3", @"Error: Invalid delimiter for \right: notadelimiter
···2\right\notadelimiter + 3
                       ↑ (pos 33)"),
      InlineData(@"5 + 3 \right)", @"Error: Missing \left
5 + 3 \right)
           ↑ (pos 12)"),
      InlineData(@"\left(\frac12", @"Error: Missing \right for \left with delimiter (
\left(\frac12
            ↑ (pos 13)"),
      InlineData(@"\left(5 + \left| \frac12 \right)", @"Error: Missing \right for \left with delimiter (
···left| \frac12 \right)
                       ↑ (pos 32)"),
      InlineData(@"5+ \left|\frac12\right| \right)", @"Error: Missing \left
···\frac12\right| \right)
                       ↑ (pos 30)"),
      InlineData(@"\begin matrix \end matrix", @"Error: Missing {
\begin matrix \end matrix
      ↑ (pos 7)"),
      InlineData(@"\begin", @"Error: Missing {
\begin
     ↑ (pos 6)"),
      InlineData(@"\begin{", @"Error: Missing }
\begin{
      ↑ (pos 7)"),
      InlineData(@"\begin{matrix parens}", @"Error: Missing }
\begin{matrix parens}
             ↑ (pos 14)"), // no spaces in env
      InlineData(@"\begin{matrix}", @"Error: Missing \end for \begin{matrix}
\begin{matrix}
             ↑ (pos 14)"),
      InlineData(@"\begin{matrix} x", @"Error: Missing \end for \begin{matrix}
\begin{matrix} x
               ↑ (pos 16)"),
      InlineData(@"\begin{matrix} x \end", @"Error: Missing {
\begin{matrix} x \end
                    ↑ (pos 21)"),
      InlineData(@"\begin{matrix} x \end + 3", @"Error: Missing {
···begin{matrix} x \end + 3
                       ↑ (pos 22)"),
      InlineData(@"\begin{matrix} x \end{", @"Error: Missing }
···begin{matrix} x \end{
                       ↑ (pos 22)"),
      InlineData(@"\begin{matrix} x \end{matrix + 3", @"Error: Missing }
···atrix} x \end{matrix + 3
                       ↑ (pos 29)"),
      InlineData(@"\begin{matrix} x \end{pmatrix}", @"Error: Begin environment name matrix does not match end environment name pmatrix
···trix} x \end{pmatrix}
                       ↑ (pos 30)"),
      InlineData(@"x \end{matrix}", @"Error: Missing \begin
x \end{matrix}
     ↑ (pos 6)"),
      InlineData(@"\begin{notanenv} x \end{notanenv}", @"Error: Unknown environment notanenv
···env} x \end{notanenv}
                       ↑ (pos 33)"),
      InlineData(@"\begin{matrix} \notacommand \end{matrix}", @"Error: Invalid command \notacommand
···{matrix} \notacommand \end{matrix}
                       ↑ (pos 27)"),
      InlineData(@"\begin{displaylines} x & y \end{displaylines}", @"Error: displaylines environment can only have 1 column
··· y \end{displaylines}
                       ↑ (pos 45)"),
      InlineData(@"\begin{eqalign} x \end{eqalign}", @"Error: eqalign environment can only have 2 columns
···lign} x \end{eqalign}
                       ↑ (pos 31)"),
      InlineData(@"\limits", @"Error: \limits can only be applied to an operator
\limits
      ↑ (pos 7)"),
      InlineData(@"\nolimits", @"Error: \nolimits can only be applied to an operator
\nolimits
        ↑ (pos 9)"),
      InlineData(@"\frac\limits{1}{2}", @"Error: \limits can only be applied to an operator
\frac\limits{1}{2}
           ↑ (pos 12)"),
      InlineData(@"\color{notacolor}{xyz}", @"Error: Invalid color: notacolor
\color{notacolor}{xyz}
               ↑ (pos 16)"),
      InlineData(@"\color{red blue}{xyz}", @"Error: Missing }
\color{red blue}{xyz}
          ↑ (pos 11)"),
      InlineData(@"\left(\begin{matrix}\right)", @"Error: Missing \end{matrix}
···(\begin{matrix}\right)
                       ↑ (pos 26)"),
      InlineData(@"\Bra^2", @"Error: ^ cannot appear as an argument to a command
\Bra^2
    ↑ (pos 5)"),
      InlineData(@"\Bra_2", @"Error: _ cannot appear as an argument to a command
\Bra_2
    ↑ (pos 5)"),
      InlineData(@"\Bra&2", @"Error: & cannot appear as an argument to a command
\Bra&2
    ↑ (pos 5)"),
      InlineData(@"\Bra}2", @"Error: } cannot appear as an argument to a command
\Bra}2
    ↑ (pos 5)"),
      InlineData(@"\Ket^2", @"Error: ^ cannot appear as an argument to a command
\Ket^2
    ↑ (pos 5)"),
      InlineData(@"\Ket_2", @"Error: _ cannot appear as an argument to a command
\Ket_2
    ↑ (pos 5)"),
      InlineData(@"\Ket&2", @"Error: & cannot appear as an argument to a command
\Ket&2
    ↑ (pos 5)"),
      InlineData(@"\Ket}2", @"Error: } cannot appear as an argument to a command
\Ket}2
    ↑ (pos 5)"),
    ]
    public void TestErrors(string badInput, string expected) {
      var (list, actual) = LaTeXParser.MathListFromLaTeX(badInput);
      Assert.Null(list);
      Assert.Equal(expected.Replace("\r", null), actual);
    }
  }
}
