using CSharpMath.Atom;
using CSharpMath.Display;
using CSharpMath.Display.Displays;
using CSharpMath.Display.FrontEnd;
using CSharpMath.Tests.FrontEnd;
using System.Drawing;
using Xunit;
using TGlyph = System.Char;
using TFont = CSharpMath.Tests.FrontEnd.TestFont;

namespace CSharpMath.Tests {
  public class DisplayTests {
    private readonly TFont _font = new TFont(20);
    private readonly TypesettingContext<TFont, TGlyph> _context = TestTypesettingContexts.Instance;

    [System.Diagnostics.DebuggerStepThrough] // Debugger should stop at the line that uses this function
    void AssertText(string expected, TextLineDisplay<TFont, TGlyph> actual) =>
      Assert.Equal(expected, string.Concat(actual.Text));

    System.Action<IDisplay<TFont, TGlyph>> TestList(int rangeMax, double ascent, double descent, double width, double x, double y,
      LinePosition linePos, int indexInParent, params System.Action<IDisplay<TFont, TGlyph>>[] inspectors) => d => {
        var list = Assert.IsType<ListDisplay<TFont, TGlyph>>(d);
        Assert.False(list.HasScript);
        Assert.Equal(new Range(0, rangeMax), list.Range);
        Approximately.Equal(ascent, list.Ascent);
        Approximately.Equal(descent, list.Descent);
        Approximately.Equal(width, list.Width);
        Approximately.At(x, y, list.Position); // may change as we implement more details?
            Assert.Equal(linePos, list.LinePosition);
        Assert.Equal(indexInParent, list.IndexInParent);
        Assert.Collection(list.Displays, inspectors);
      };
    void TestOuter(string latex, int rangeMax, double ascent, double descent, double width,
        params System.Action<IDisplay<TFont, TGlyph>>[] inspectors) =>
      TestList(rangeMax, ascent, descent, width, 0, 0, LinePosition.Regular, Range.UndefinedInt, inspectors)
      (Typesetter.CreateLine(LaTeXBuilder.MathListFromLaTeX(latex), _font, _context, LineStyle.Display));

    [Theory, InlineData("x"), InlineData("2")]
    public void TestSimpleVariable(string latex) =>
      TestOuter(latex, 1, 14, 4, 10,
        d => {
          var line = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
          Assert.Single(line.Atoms); // have to think about these; doesn't really work atm
          AssertText(latex, line);
          Assert.Equal(new PointF(), line.Position);
          Assert.Equal(new Range(0, 1), line.Range);

          Assert.False(line.HasScript);
          Assert.Equal(14, line.Ascent);
          Assert.Equal(4, line.Descent);
          Assert.Equal(10, line.Width);
        });

    [Theory, InlineData("xyzw"), InlineData("xy2w"), InlineData("1234")]
    public void TestVariablesAndNumbers(string latex) =>
      TestOuter(latex, 4, 14, 4, 40,
        d => {
          var line = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
          Assert.Equal(4, line.Atoms.Count);
          AssertText(latex, line);
          Assert.Equal(new PointF(), line.Position);
          Assert.Equal(new Range(0, 4), line.Range);
          Assert.False(line.HasScript);

          Assert.Equal(14, line.Ascent);
          Assert.Equal(4, line.Descent);
          Assert.Equal(40, line.Width);
        });

    [Fact]
    public void TestSuperScript() =>
      TestOuter("x^2", 1, 17.06, 4, 17.32,
        d => {
          var line = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
          Assert.Single(line.Atoms);
          AssertText("x", line);
          Assert.Equal(new PointF(), line.Position);
          Assert.True(line.HasScript);
        },
        TestList(1, 9.8, 2.8, 7, 10.32, 7.26, LinePosition.Superscript, 0,
          d => {
            var super10 = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
            Assert.NotNull(super10);
            Assert.Single(super10.Atoms);
            Assert.Equal(new PointF(), super10.Position);
            Assert.False(super10.HasScript);
          }));

    [Fact]
    public void TestSuperScriptEmptyBase() =>
      TestOuter("^2", 0, 17.06, 0, 7,
        d => {
          var line = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
          Assert.Single(line.Atoms);
          AssertText("", line);
          Assert.Equal(new PointF(), line.Position);
          Assert.True(line.HasScript);
        },
        TestList(1, 9.8, 2.8, 7, 0, 7.26, LinePosition.Superscript, 0,
          d => {
            var super10 = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
            Assert.NotNull(super10);
            Assert.Single(super10.Atoms);
            Assert.Equal(new PointF(), super10.Position);
            Assert.False(super10.HasScript);
          }));

    [Fact]
    public void TestSubscript() =>
      TestOuter("x_1", 1, 14, 7.74, 17,
        d => {
          var line = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
          Assert.Single(line.Atoms);
          AssertText("x", line);
          Assert.Equal(new PointF(), line.Position);
          Assert.True(line.HasScript);
        },
        TestList(1, 9.8, 2.8, 7, 10, -4.94, LinePosition.Subscript, 0,
          d => {
            var sub10 = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
            Assert.NotNull(sub10);
            Assert.Single(sub10.Atoms);
            Assert.Equal(new PointF(), sub10.Position);
            Assert.False(sub10.HasScript);
          }));

    [Fact]
    public void TestSuperSubscript() =>
      TestOuter("x^2_1", 1, 19.48, 8.92, 17.32,
        d => {
          var line = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
          Assert.Single(line.Atoms);
          AssertText("x", line);
          Assert.Equal(new PointF(), line.Position);
          Assert.True(line.HasScript);
        },
        TestList(1, 9.8, 2.8, 7, 10.32, 9.68, LinePosition.Superscript, 0,
          d => {
            var line2 = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
            Assert.Single(line2.Atoms);
            AssertText("2", line2);
            Assert.Equal(new PointF(), line2.Position);
            Assert.False(line2.HasScript);
            Approximately.Equal(20.12, 10.32 + line2.Ascent);
          }),
        // Because both subscript and superscript are present, coords are
        // different from the subscript-only case.
        TestList(1, 9.8, 2.8, 7, 10, -6.12, LinePosition.Subscript, 0,
          d => {
            var line3 = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
            Assert.Single(line3.Atoms);
            AssertText("1", line3);
            Assert.Equal(new PointF(), line3.Position);
            Assert.False(line3.HasScript);
            Approximately.Equal(8.92, line3.Descent - (-6.12));
          }));
    [Theory, InlineData("\\binom13"), InlineData("1\\choose3")]
    public void TestBinomial(string latex) =>
      TestOuter(latex, 1, 27.54, 17.72, 30,
        TestList(1, 27.54, 17.72, 30, 0, 0, LinePosition.Regular, Range.UndefinedInt,
          d => {
            var glyph = Assert.IsType<GlyphDisplay<TFont, TGlyph>>(d);
            Assert.Equal(new PointF(), glyph.Position);
            Assert.Equal(Range.NotFound, glyph.Range);
            Assert.False(glyph.HasScript);
          },
          d => {
            var subFraction = Assert.IsType<FractionDisplay<TFont, TGlyph>>(d);
            Assert.Equal(new Range(0, 1), subFraction.Range);
            Assert.False(subFraction.HasScript);
            Approximately.At(10, 0, subFraction.Position);
            TestList(1, 14, 4, 10, 10, 13.54, LinePosition.Regular, Range.UndefinedInt,
              dd => {
              var subNumerator = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(dd);
              Assert.Single(subNumerator.Atoms);
              AssertText("1", subNumerator);
              Assert.Equal(new PointF(), subNumerator.Position);
              Assert.Equal(new Range(0, 1), subNumerator.Range);
              Assert.False(subNumerator.HasScript);
            })(subFraction.Numerator);
            TestList(1, 14, 4, 10, 10, -13.72, LinePosition.Regular, Range.UndefinedInt,
              dd => {
              var subDenominator = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(dd);
              Assert.Single(subDenominator.Atoms);
              AssertText("3", subDenominator);
              Assert.Equal(new PointF(), subDenominator.Position);
              Assert.Equal(new Range(0, 1), subDenominator.Range);
              Assert.False(subDenominator.HasScript);
            })(subFraction.Denominator);
          },
          d => {
            var subRight = Assert.IsType<GlyphDisplay<TFont, TGlyph>>(d);
            Assert.False(subRight.HasScript);
            Assert.Equal(Range.NotFound, subRight.Range);
            Approximately.At(20, 0, subRight.Position);
          }));

    [Theory, InlineData("\\frac13", 0.8), InlineData("1\\atop3", 0)]
    public void TestFraction(string latex, double lineThickness) =>
      TestOuter(latex, 1, 27.54, 17.72, 10,
        d => {
          var fraction = Assert.IsType<FractionDisplay<TFont, TGlyph>>(d);
          Assert.Equal(new Range(0, 1), fraction.Range);
          Assert.Equal(new PointF(), fraction.Position);
          Assert.False(fraction.HasScript);
          Approximately.Equal(lineThickness, fraction.LineThickness);

          TestList(1, 14, 4, 10, 0, 13.54, LinePosition.Regular, Range.UndefinedInt,
            dd => {
            var subNumerator = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(dd);
            Assert.Single(subNumerator.Atoms);
            AssertText("1", subNumerator);
            Assert.Equal(new PointF(), subNumerator.Position);
            Assert.Equal(new Range(0, 1), subNumerator.Range);
            Assert.False(subNumerator.HasScript);

          })(fraction.Numerator);

          TestList(1, 14, 4, 10, 0, -13.72, LinePosition.Regular, Range.UndefinedInt,
            dd => {
            var subDenominator = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(dd);
            Assert.Single(subDenominator.Atoms);
            AssertText("3", subDenominator);
            Assert.Equal(new PointF(), subDenominator.Position);
            Assert.Equal(new Range(0, 1), subDenominator.Range);
            Assert.False(subDenominator.HasScript);
          })(fraction.Denominator);
        });
    [Theory, InlineData("2x+3=y"), InlineData("y=3+2x")]
    public void TestEquationWithOperatorsAndRelations(string latex) =>
      TestOuter(latex, 6, 14, 4, 80, d => {
        var line = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);

        Assert.Equal(6, line.Atoms.Count);
        AssertText(latex, line);
        Assert.Equal(new PointF(), line.Position);
        Assert.Equal(new Range(0, 6), line.Range);
        Assert.False(line.HasScript);

        Assert.Equal(14, line.Ascent);
        Assert.Equal(4, line.Descent);
        Assert.Equal(80, line.Width);
      });

    [Theory, InlineData('[', ']'), InlineData('(', '}'), InlineData('{', ']')] // Using ) confuses the test explorer...
    public void TestInner(char left, char right) =>
      TestOuter($@"\left{left}x\right{right}", 1, 14, 4, 30,
        TestList(1, 14, 4, 30, 0, 0, LinePosition.Regular, Range.UndefinedInt,
          d => {
            var glyph = Assert.IsType<GlyphDisplay<TFont, TGlyph>>(d);
            Assert.Equal(new PointF(), glyph.Position);
            Assert.Equal(Range.NotFound, glyph.Range);
            Assert.False(glyph.HasScript);
            Assert.Equal(left, glyph.Glyph);
          },
          TestList(1, 14, 4, 10, 10, 0, LinePosition.Regular, Range.UndefinedInt,
            d => {
              var line = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
              Assert.Single(line.Atoms);
              AssertText("x", line);
              Assert.Equal(new PointF(), line.Position);
              Assert.False(line.HasScript);
            }),
          d => {
            var glyph2 = Assert.IsType<GlyphDisplay<TFont, TGlyph>>(d);
            Approximately.At(20, 0, glyph2.Position);
            Assert.Equal(Range.NotFound, glyph2.Range);
            Assert.False(glyph2.HasScript);
            Assert.Equal(right, glyph2.Glyph);
          }));
    [Theory, InlineData("\\sqrt2", null, "2"), InlineData("\\sqrt[3]2", "3", "2")]
    public void TestRadical(string latex, string degree, string radicand) =>
      TestOuter(latex, 1, 18.56, 4, degree is null ? 20 : 21.44, d => {
        var radical = Assert.IsType<RadicalDisplay<TFont, TGlyph>>(d);
        Assert.Equal(new Range(0, 1), radical.Range);
        Assert.False(radical.HasScript);
        Assert.Equal(new PointF(), radical.Position);
        Assert.NotNull(radical.Radicand);

        if (degree != null)
          TestList(1, 9.8, 2.8, 7, 5.56, 8.736, LinePosition.Regular, Range.UndefinedInt, dd => {
            var line3 = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(dd);
            Assert.Single(line3.Atoms);
            AssertText(degree, line3);
            Assert.Equal(new PointF(), line3.Position);
            Assert.Equal(new Range(0, 1), line3.Range);
            Assert.False(line3.HasScript);
          })(radical.Degree);
        else Assert.Null(radical.Degree);

        TestList(1, 14, 4, 10, degree is null ? 10 : 11.44, 0, LinePosition.Regular, Range.UndefinedInt, dd => {
          var line2 = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(dd);
          Assert.Single(line2.Atoms);
          AssertText(radicand, line2);
          Assert.Equal(new PointF(), line2.Position);
          Assert.Equal(new Range(0, 1), line2.Range);
          Assert.False(line2.HasScript);
        })(radical.Radicand);
      });

    [Theory, InlineData(3), InlineData(-3), InlineData(0.1), InlineData(-0.1)]
    public void TestRaiseBox(double height) =>
      TestOuter($@"\text\raisebox{{{height}pt}}r", 1, 14 + height, 4 - height, 10,
        TestList(1, 14, 4, 10, 0, height, LinePosition.Regular, Range.UndefinedInt,
          d => {
            var line = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
            Assert.Single(line.Atoms);
            AssertText("r", line);
            Assert.Equal(new PointF(), line.Position);
            Assert.False(line.HasScript);
          }));
    [Fact]
    public void TestLargeOperator() =>
      TestOuter(@"\int^\pi_0 \theta d\theta", 4, 19.48, 8.92, 51.453,
        TestList(1, 9.8, 2.8, 7, 10, 9.68, LinePosition.Superscript, 0,
          d => {
            var superscript = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
            AssertText("π", superscript);
            Assert.Single(superscript.Atoms);
            Assert.Equal(new PointF(), superscript.Position);
            Assert.False(superscript.HasScript);
          }),
        TestList(1, 9.8, 2.8, 7, 10, -6.12, LinePosition.Subscript, 0,
          d => {
            var subscript = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
            AssertText("0", subscript);
            Assert.Single(subscript.Atoms);
            Assert.Equal(new PointF(), subscript.Position);
            Assert.False(subscript.HasScript);
          }),
        d => {
          var glyph = Assert.IsType<GlyphDisplay<TFont, TGlyph>>(d);
          Assert.Equal('∫', glyph.Glyph);
          Assert.Equal(new PointF(), glyph.Position);
          Assert.True(glyph.HasScript);
        },
        d => {
          var textAfter = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
          AssertText("θdθ", textAfter);
        });
    [Fact]
    public void TestLargeOperatorLimits() =>
      TestOuter(@"\int\limits^\pi_0 \theta d\theta", 4, 30.6, 19.94, 43.333,
        d => {
          var largeOp = Assert.IsType<LargeOpLimitsDisplay<TFont, TGlyph>>(d);
          var glyph = Assert.IsType<GlyphDisplay<TFont, TGlyph>>(largeOp.NucleusDisplay);
          Assert.Equal('∫', glyph.Glyph);
          Assert.Equal(new PointF(), glyph.Position);
          Assert.False(glyph.HasScript);
          TestList(1, 9.8, 2.8, 7, 1.5, 20.8, LinePosition.Regular, Range.UndefinedInt,
            d => {
              var superscript = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
              AssertText("π", superscript);
              Assert.Single(superscript.Atoms);
              Assert.Equal(new PointF(), superscript.Position);
              Assert.False(superscript.HasScript);
            })(largeOp.UpperLimit);
          TestList(1, 9.8, 2.8, 7, 1.5, -17.14, LinePosition.Regular, Range.UndefinedInt,
            d => {
              var subscript = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
              AssertText("0", subscript);
              Assert.Single(subscript.Atoms);
              Assert.Equal(new PointF(), subscript.Position);
              Assert.False(subscript.HasScript);
            })(largeOp.LowerLimit);
        },
        d => {
          var textAfter = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(d);
          AssertText("θdθ", textAfter);
        });
  }
}