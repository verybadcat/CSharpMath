using CSharpMath.Atoms;
using CSharpMath.Display;
using CSharpMath.Display.Text;
using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using CSharpMath.Tests.FrontEnd;
using System.Drawing;
using System.Linq;
using Xunit;
using TGlyph = System.Char;
using TFont = CSharpMath.Tests.FrontEnd.TestFont;

namespace CSharpMath.Tests {
  public class TypesettingTests {
    private readonly TFont _font = new TFont(20);
    private readonly TypesettingContext<TFont, TGlyph> _context = TestTypesettingContexts.Instance;

    [System.Diagnostics.DebuggerStepThrough] // Debugger should stop at the line that uses this function
    void AssertText(string expected, TextLineDisplay<TFont, TGlyph> actual) =>
      Assert.Equal(expected, string.Concat(actual.Text));

    void Test(MathList input, Range range, bool hasScript, System.Action<IDisplay<TFont, TGlyph>>[] forEach) {

    }

    [Fact]
    public void TestSimpleVariable() {
      var list = new MathList {
        MathAtoms.ForCharacter('x')
      };
      var display = _context.CreateLine(list, _font, LineStyle.Display);
      Assert.NotNull(display);
      Assert.Equal(LinePosition.Regular, display.LinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(int.MinValue, display.IndexInParent);
      Assert.Single(display.Displays);
      var sub0 = display.Displays[0];
      Assert.True(sub0 is TextLineDisplay<TFont, TGlyph>);
      var line = sub0 as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(line.Atoms); // have to think about these; doesn't really work atm

      AssertText("x", line);
      Assert.Equal(new PointF(), line.Position);
      Assert.Equal(new Range(0, 1), line.Range);
      Assert.False(line.HasScript);
      Approximately.Equal(14, display.Ascent);
      Approximately.Equal(4, display.Descent);
      Approximately.Equal(10, display.Width);
      Assert.Equal(display.Ascent, line.Ascent);
      Assert.Equal(display.Descent, line.Descent);
      Assert.Equal(display.Width, line.Width);
    }

    [Fact]
    public void TestMultipleVariables() {
      var list = MathLists.FromString("xyzw");
      var display = Typesetter<TFont, TGlyph>.CreateLine(list, _font, _context, LineStyle.Display);

      Assert.NotNull(display);

      Assert.Equal(LinePosition.Regular, display.LinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 4), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var line = display.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.NotNull(line);
      Assert.Equal(4, line.Atoms.Length);

      AssertText("xyzw", line);
      Assert.Equal(new PointF(), line.Position);

      Assert.Equal(new Range(0, 4), line.Range);
      Assert.False(line.HasScript);
      Assert.Equal(display.Ascent, line.Ascent);
      Assert.Equal(display.Descent, line.Descent);
      Assert.Equal(display.Width, line.Width);
      Approximately.Equal(14, display.Ascent);
      Approximately.Equal(4, display.Descent);
      Approximately.Equal(40, display.Width);
    }

    [Fact]
    public void TestVariablesAndNumbers() {
      var mathList = MathLists.FromString("xy2w");

      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.NotNull(display);
      Assert.Equal(LinePosition.Regular, display.LinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 4), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var line = display.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.NotNull(line);
      Assert.Equal(4, line.Atoms.Length);
      AssertText("xy2w", line);
      Assert.Equal(new PointF(), line.Position);
      Assert.Equal(new Range(0, 4), line.Range);
      Assert.False(line.HasScript);

      Assert.Equal(display.Ascent, line.Ascent);
      Assert.Equal(display.Descent, line.Descent);
      Assert.Equal(display.Width, line.Width);
      Approximately.Equal(14, display.Ascent);
      Approximately.Equal(4, display.Descent);
      Approximately.Equal(40, display.Width);
    }

    [Fact]
    public void TestSuperScript() {
      var x = MathAtoms.ForCharacter('x');
      x.Superscript = new MathList { MathAtoms.ForCharacter('2') };
      var mathList = new MathList { x };

      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.NotNull(display);

      Assert.Equal(LinePosition.Regular, display.LinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(display.IndexInParent, Range.UndefinedInt);
      Assert.Equal(2, display.Displays.Count());

      var line = display.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.NotNull(line);
      Assert.Single(line.Atoms);
      AssertText("x", line);
      Assert.Equal(new PointF(), line.Position);
      Assert.True(line.HasScript);

      var super1 = display.Displays[1] as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(super1);
      Assert.Equal(LinePosition.Superscript, super1.LinePosition);
      var super1Position = super1.Position;
      Approximately.At(10.32, 7.26, super1Position); // may change as we implement more details?
      Assert.Equal(new Range(0, 1), super1.Range);
      Assert.False(super1.HasScript);
      Assert.Equal(0, super1.IndexInParent);
      Assert.Single(super1.Displays);

      var super10 = super1.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.NotNull(super10);
      Assert.Single(super10.Atoms);
      Assert.Equal(new PointF(), super10.Position);
      Assert.False(super10.HasScript);

      Approximately.Equal(17.06, display.Ascent);
      Approximately.Equal(4, display.Descent);
    }

    [Fact]
    public void TestSubscript() {
      var x = MathAtoms.ForCharacter('x');
      x.Subscript = new MathList { MathAtoms.ForCharacter('1') };
      var mathList = new MathList { x };

      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.NotNull(display);

      Assert.Equal(LinePosition.Regular, display.LinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(display.IndexInParent, Range.UndefinedInt);
      Assert.Equal(2, display.Displays.Count());

      var line = display.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.NotNull(line);
      Assert.Single(line.Atoms);
      AssertText("x", line);
      Assert.Equal(new PointF(), line.Position);
      Assert.True(line.HasScript);
      var sub1 = display.Displays[1] as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(sub1);
      Assert.Equal(LinePosition.Subscript, sub1.LinePosition);
      Approximately.At(10, -4.94, sub1.Position); // may change as we implement more details?
      Assert.Equal(new Range(0, 1), sub1.Range);
      Assert.False(sub1.HasScript);
      Assert.Equal(0, sub1.IndexInParent);
      Assert.Single(sub1.Displays);

      var sub10 = sub1.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.NotNull(sub10);
      Assert.Single(sub10.Atoms);
      Assert.Equal(new PointF(), sub10.Position);
      Assert.False(sub10.HasScript);

      Approximately.Equal(14, display.Ascent);
      Approximately.Equal(7.74, display.Descent);
    }

    [Fact]
    public void TestSuperSubscript() {
      var x = MathAtoms.ForCharacter('x');
      x.Subscript = new MathList { MathAtoms.ForCharacter('1') };
      x.Superscript = new MathList { MathAtoms.ForCharacter('2') };
      var mathList = new MathList { x };

      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.NotNull(display);
      Assert.Equal(LinePosition.Regular, display.LinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Equal(3, display.Displays.Count());

      var line = display.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(line.Atoms);
      AssertText("x", line);
      Assert.Equal(new PointF(), line.Position);
      Assert.True(line.HasScript);

      var display2 = display.Displays[1] as ListDisplay<TFont, TGlyph>;
      Assert.Equal(LinePosition.Superscript, display2.LinePosition);
      Approximately.At(10.32, 9.68, display2.Position);
      Assert.Equal(new Range(0, 1), display2.Range);
      Assert.False(display2.HasScript);
      Assert.Equal(0, display2.IndexInParent);
      Assert.Single(display2.Displays);

      var line2 = display2.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(line2.Atoms);
      AssertText("2", line2);
      Assert.Equal(new PointF(), line2.Position);
      Assert.False(line2.HasScript);

      var display3 = display.Displays[2] as ListDisplay<TFont, TGlyph>;
      Assert.Equal(LinePosition.Subscript, display3.LinePosition);

      // Because both subscript and superscript are present, coords are
      // different from the subscript-only case.
      Approximately.At(10, -6.12, display3.Position);
      Assert.Equal(new Range(0, 1), display3.Range);
      Assert.False(display3.HasScript);
      Assert.Equal(0, display3.IndexInParent);
      Assert.Single(display3.Displays);

      var line3 = display3.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(line3.Atoms);
      AssertText("1", line3);
      Assert.Equal(new PointF(), line3.Position);
      Assert.False(line3.HasScript);

      Approximately.Equal(19.48, display.Ascent);
      Approximately.Equal(8.92, display.Descent);
      Approximately.Equal(17.32, display.Width);
      Approximately.Equal(display.Ascent, display2.Position.Y + line2.Ascent);
      Approximately.Equal(display.Descent, line3.Descent - display3.Position.Y);
    }
    [Fact]
    public void TestBinomial() {
      var list = new MathList {
        new Fraction(false) {
          LeftDelimiter = "(",
          RightDelimiter = ")",
          Numerator = new MathList {
            MathAtoms.ForCharacter('1')
          },
          Denominator = new MathList {
            MathAtoms.ForCharacter('3')
          }
        }
      };

      var display = Typesetter<TFont, TGlyph>.CreateLine(list, _font, _context, LineStyle.Display);
      Assert.Equal(LinePosition.Regular, display.LinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var display0 = display.Displays[0] as ListDisplay<TFont, TGlyph>;
      Assert.Equal(LinePosition.Regular, display0.LinePosition);
      Assert.Equal(new PointF(), display0.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display0.HasScript);
      Assert.Equal(Range.UndefinedInt, display0.IndexInParent);
      Assert.Equal(3, display0.Displays.Count);

      var glyph = display0.Displays[0] as GlyphDisplay<TFont, TGlyph>;
      Assert.Equal(new PointF(), glyph.Position);
      Assert.Equal(Range.NotFound, glyph.Range);
      Assert.False(glyph.HasScript);

      var subFraction = display0.Displays[1] as FractionDisplay<TFont, TGlyph>;
      Assert.Equal(new Range(0, 1), subFraction.Range);
      Assert.False(subFraction.HasScript);
      Approximately.At(10, 0, subFraction.Position);
      
      var numerator = subFraction.Numerator as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(numerator);
      Assert.Equal(LinePosition.Regular, numerator.LinePosition);
      Approximately.At(10, 13.54, numerator.Position);
      Assert.Single(numerator.Displays);
      Assert.Equal(Range.UndefinedInt, numerator.IndexInParent);
      Assert.False(numerator.HasScript);

      var subNumerator = numerator.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(subNumerator.Atoms);
      AssertText("1", subNumerator);
      Assert.Equal(new PointF(), subNumerator.Position);
      Assert.Equal(new Range(0, 1), subNumerator.Range);
      Assert.False(subNumerator.HasScript);

      var denominator = subFraction.Denominator as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(denominator);
      Assert.Equal(LinePosition.Regular, denominator.LinePosition);
      Approximately.At(10, -13.72, denominator.Position);
      Assert.Equal(new Range(0, 1), denominator.Range);
      Assert.False(denominator.HasScript);
      Assert.Equal(Range.UndefinedInt, denominator.IndexInParent);
      Assert.Single(denominator.Displays);

      var subDenominator = denominator.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(subDenominator.Atoms);
      AssertText("3", subDenominator);
      Assert.Equal(new PointF(), subDenominator.Position);
      Assert.Equal(new Range(0, 1), subDenominator.Range);
      Assert.False(subDenominator.HasScript);

      var subRight = display0.Displays[2] as GlyphDisplay<TFont, TGlyph>;
      Assert.False(subRight.HasScript);
      Assert.Equal(Range.NotFound, subRight.Range);
      Approximately.At(20, 0, subRight.Position);
      Approximately.Equal(27.54, display.Ascent);
      Approximately.Equal(17.72, display.Descent);
      Approximately.Equal(30, display.Width);
    }

    [Fact]
    public void TestFraction() {
      var mathList = new MathList {
        new Fraction(true) {
          Numerator = new MathList {
            MathAtoms.ForCharacter('1')
          },
          Denominator = new MathList {
            MathAtoms.ForCharacter('3')
          }
        }
      };

      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.Equal(LinePosition.Regular, display.LinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var fraction = display.Displays[0] as FractionDisplay<TFont, TGlyph>;
      Assert.Equal(new Range(0, 1), fraction.Range);
      Assert.Equal(new PointF(), fraction.Position);
      Assert.False(fraction.HasScript);

      var numerator = fraction.Numerator as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(numerator);
      Assert.Equal(LinePosition.Regular, numerator.LinePosition);
      Approximately.At(0, 13.54, numerator.Position);
      Assert.Equal(new Range(0, 1), numerator.Range);
      Assert.False(numerator.HasScript);
      Assert.Equal(Range.UndefinedInt, numerator.IndexInParent);
      Assert.Single(numerator.Displays);

      var subNumerator = numerator.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(subNumerator.Atoms);
      AssertText("1", subNumerator);
      Assert.Equal(new PointF(), subNumerator.Position);
      Assert.Equal(new Range(0, 1), subNumerator.Range);
      Assert.False(subNumerator.HasScript);

      var denominator = fraction.Denominator as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(denominator);
      Assert.Equal(LinePosition.Regular, denominator.LinePosition);
      Approximately.At(0, -13.72, denominator.Position);
      Assert.Equal(new Range(0, 1), denominator.Range);
      Assert.False(denominator.HasScript);
      Assert.Equal(Range.UndefinedInt, denominator.IndexInParent);
      Assert.Single(denominator.Displays);

      var subDenominator = denominator.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(subDenominator.Atoms);
      AssertText("3", subDenominator);
      Assert.Equal(new PointF(), subDenominator.Position);
      Assert.Equal(new Range(0, 1), subDenominator.Range);
      Assert.False(subDenominator.HasScript);

      Approximately.Equal(27.54, display.Ascent);
      Approximately.Equal(17.72, display.Descent);
      Approximately.Equal(10, display.Width);
    }

    [Fact]
    public void TestEquationWithOperatorsAndRelations() {
      var mathList = MathLists.FromString("2x+3=y");
      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.Equal(LinePosition.Regular, display.LinePosition);
      Assert.Equal(new Range(0, 6), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var line = display.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Equal(6, line.Atoms.Length);
      AssertText("2x+3=y", line);
      Assert.Equal(new PointF(), line.Position);
      Assert.Equal(new Range(0, 6), line.Range);
      Assert.False(line.HasScript);

      Assert.Equal(display.Ascent, line.Ascent);
      Assert.Equal(display.Width, line.Width);
      Assert.Equal(display.Descent, line.Descent);

      Approximately.Equal(14, display.Ascent);
      Approximately.Equal(4, display.Descent);
      Approximately.Equal(80, display.Width);
    }

    [Fact]
    public void TestAtop() {
      var mathList = new MathList {
        new Fraction(false) {
          Numerator = new MathList {
            MathAtoms.ForCharacter('1')
          },
          Denominator = new MathList {
            MathAtoms.ForCharacter('3')
          }
        }
      };

      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.Equal(LinePosition.Regular, display.LinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var fraction = display.Displays[0] as FractionDisplay<TFont, TGlyph>;
      Assert.Equal(new Range(0, 1), fraction.Range);
      Assert.False(fraction.HasScript);
      Assert.Equal(new PointF(), fraction.Position);

      var numerator = fraction.Numerator as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(numerator);
      Assert.Equal(LinePosition.Regular, numerator.LinePosition);
      Assert.False(numerator.HasScript);
      Approximately.At(0, 13.54, numerator.Position);
      Assert.Equal(new Range(0, 1), numerator.Range);
      Assert.Equal(Range.UndefinedInt, numerator.IndexInParent);
      Assert.Single(numerator.Displays);

      var subNumerator = numerator.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(subNumerator.Atoms);
      AssertText("1", subNumerator);
      Assert.Equal(new PointF(), subNumerator.Position);
      Assert.Equal(new Range(0, 1), subNumerator.Range);
      Assert.False(subNumerator.HasScript);

      var denominator = fraction.Denominator as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(denominator);
      Assert.Equal(LinePosition.Regular, denominator.LinePosition);
      Approximately.At(0, -13.72, denominator.Position);
      Assert.Equal(new Range(0, 1), denominator.Range);
      Assert.False(denominator.HasScript);
      Assert.Equal(Range.UndefinedInt, denominator.IndexInParent);
      Assert.Single(denominator.Displays);

      var subDenominator = denominator.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(subDenominator.Atoms);
      AssertText("3", subDenominator);
      Assert.Equal(new PointF(), subDenominator.Position);
      Assert.Equal(new Range(0, 1), subDenominator.Range);
      Assert.False(subDenominator.HasScript);

      Approximately.Equal(27.54, display.Ascent);
      Approximately.Equal(17.72, display.Descent);
      Approximately.Equal(10, display.Width);
    }

    [Fact]
    public void TestInner() {
      var mathList = new MathList {
        new Inner {
          InnerList = new MathList {
            MathAtoms.ForCharacter('x'),
          },
          LeftBoundary = MathAtoms.Create(MathAtomType.Boundary, '('),
          RightBoundary = MathAtoms.Create(MathAtomType.Boundary, ')')
        }
      };

      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.Equal(LinePosition.Regular, display.LinePosition);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var display2 = display.Displays[0] as ListDisplay<TFont, TGlyph>;
      Assert.Equal(LinePosition.Regular, display2.LinePosition);
      Assert.Equal(new PointF(), display2.Position);
      Assert.Equal(new Range(0, 1), display2.Range);
      Assert.False(display2.HasScript);
      Assert.Equal(Range.UndefinedInt, display2.IndexInParent);
      Assert.Equal(3, display2.Displays.Count);

      var glyph = display2.Displays[0] as GlyphDisplay<TFont, TGlyph>;
      Assert.Equal(new PointF(), glyph.Position);
      Assert.Equal(Range.NotFound, glyph.Range);
      Assert.False(glyph.HasScript);

      var display3 = display2.Displays[1] as ListDisplay<TFont, TGlyph>;
      Assert.Equal(LinePosition.Regular, display3.LinePosition);
      Approximately.At(10, 0, display3.Position);
      Assert.Equal(new Range(0, 1), display3.Range);
      Assert.False(display3.HasScript);
      Assert.Equal(Range.UndefinedInt, display3.IndexInParent);
      Assert.Single(display3.Displays);

      var line = display3.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(line.Atoms);
      AssertText("x", line);
      Assert.Equal(new PointF(), line.Position);
      Assert.False(line.HasScript);

      var glyph2 = display2.Displays[2] as GlyphDisplay<TFont, TGlyph>;
      Approximately.At(20, 0, glyph2.Position);
      Assert.Equal(Range.NotFound, glyph2.Range);
      Assert.False(glyph2.HasScript);

      Assert.Equal(display.Ascent, display2.Ascent);
      Assert.Equal(display.Descent, display2.Descent);
      Assert.Equal(display.Width, display2.Width);

      Approximately.Equal(14, display.Ascent);
      Approximately.Equal(4, display.Descent);
      Approximately.Equal(30, display.Width);
    }

    [Fact]
    public void TestRadical() {
      var mathList = new MathList {
        new Radical {
          Radicand = new MathList {
            MathAtoms.ForCharacter('1')
          }
        }
      };

      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.Equal(LinePosition.Regular, display.LinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var radical = display.Displays[0] as RadicalDisplay<TFont, TGlyph>;
      Assert.Equal(new Range(0, 1), radical.Range);
      Assert.False(radical.HasScript);
      Assert.Equal(new PointF(), radical.Position);
      Assert.NotNull(radical.Radicand);
      Assert.Null(radical.Degree);

      var display2 = radical.Radicand as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(display2);
      Assert.Equal(LinePosition.Regular, display2.LinePosition);
      Approximately.At(10, 0, display2.Position);
      Assert.Equal(new Range(0, 1), display2.Range);
      Assert.False(display2.HasScript);
      Assert.Equal(Range.UndefinedInt, display2.IndexInParent);
      Assert.Single(display2.Displays);

      var line2 = display2.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(line2.Atoms);
      AssertText("1", line2);
      Assert.Equal(new PointF(), line2.Position);
      Assert.Equal(new Range(0, 1), line2.Range);
      Assert.False(line2.HasScript);

      Approximately.Equal(18.56, display.Ascent);
      Approximately.Equal(4, display.Descent);
      Approximately.Equal(20, display.Width);
    }

    [Fact]
    public void TestRaiseBox() {
      var mathList = new MathList {
        new Atoms.Extension.RaiseBox {
          InnerList = new MathList{
            MathAtoms.ForCharacter('r')
          },
          Raise = new Space(3 * Structures.Space.Point)
        }
      };
      
      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.Equal(LinePosition.Regular, display.LinePosition);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var display2 = display.Displays[0] as ListDisplay<TFont, TGlyph>;
      Assert.Equal(LinePosition.Regular, display2.LinePosition);
      Assert.Equal(new PointF(0, 3), display2.Position);
      Assert.Equal(new Range(0, 1), display2.Range);
      Assert.False(display2.HasScript);
      Assert.Equal(Range.UndefinedInt, display2.IndexInParent);
      Assert.Equal(1, display2.Displays.Count);
      
      var line = display2.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(line.Atoms);
      AssertText("r", line);
      Assert.Equal(new PointF(), line.Position);
      Assert.False(line.HasScript);

      Approximately.Equal(17, display.Ascent);
      Approximately.Equal(1, display.Descent);
      Approximately.Equal(10, display.Width);
    }

    [Fact]
    public void TestLargeOperator() {
      var mathList = MathLists.FromString(@"\int^\pi_0 \theta d\theta");

      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.Equal(LinePosition.Regular, display.LinePosition);
      Assert.Equal(new Range(0, 4), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Collection(display.Displays,
        d => {
          var dd = Assert.IsType<ListDisplay<TFont, TGlyph>>(d);
          var superscript = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(Assert.Single(dd.Displays));
          AssertText("π", superscript);
          Assert.Single(superscript.Atoms);
          Assert.Equal(new PointF(), superscript.Position);
          Assert.False(superscript.HasScript);
        },
        d => {
          var dd = Assert.IsType<ListDisplay<TFont, TGlyph>>(d);
          var subscript = Assert.IsType<TextLineDisplay<TFont, TGlyph>>(Assert.Single(dd.Displays));
          AssertText("0", subscript);
          Assert.Single(subscript.Atoms);
          Assert.Equal(new PointF(), subscript.Position);
          Assert.False(subscript.HasScript);
        },
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

      Approximately.Equal(19.48, display.Ascent);
      Approximately.Equal(8.92, display.Descent);
      Approximately.Equal(51.453, display.Width);
    }
  }
}
