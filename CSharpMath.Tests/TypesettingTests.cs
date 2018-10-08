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
using TFont = CSharpMath.Tests.FrontEnd.TestMathFont;

namespace CSharpMath.Tests {
  public class TypesettingTests {
    public TypesettingTests() {

    }
    private TFont _font { get; } = new TFont(20);
    private TypesettingContext<TFont, char> _context { get; } = TestTypesettingContexts.Create();
    [Fact]
    public void TestSimpleVariable() {
      var list = new MathList {
        MathAtoms.ForCharacter('x')
      };
      var display = _context.CreateLine(list, _font, LineStyle.Display);
      Assert.NotNull(display);
      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(int.MinValue, display.IndexInParent);
      Assert.Single(display.Displays);
      var sub0 = display.Displays[0];
      Assert.True(sub0 is TextLineDisplay<TFont, TGlyph>);
      var line = sub0 as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(line.Atoms); // have to think about these; doesn't really work atm

      Assert.Equal("x", line.StringText());
      Assert.Equal(new PointF(), line.Position);
      Assert.Equal(new Range(0, 1), line.Range);
      Assert.False(line.HasScript);
      var descent = display.Descent;
      Assertions.ApproximatelyEqual(14, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(4, descent, 0.01);
      Assertions.ApproximatelyEqual(10, display.Width, 0.01);
      Assert.Equal(display.Ascent, line.Ascent);
      Assert.Equal(display.Descent, line.Descent);
      Assert.Equal(display.Width, line.Width);
    }

    [Fact]
    public void TestMultipleVariables() {
      var list = MathLists.FromString("xyzw");
      var display = Typesetter<TFont, TGlyph>.CreateLine(list, _font, _context, LineStyle.Display);

      Assert.NotNull(display);

      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 4), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var subDisplay = display.Displays[0];
      var line = subDisplay as TextLineDisplay<TFont, TGlyph>;
      Assert.NotNull(line);
      Assert.Equal(4, line.Atoms.Length);

      Assert.Equal("xyzw", line.StringText());
      Assert.Equal(new PointF(), line.Position);

      Assert.Equal(new Range(0, 4), line.Range);
      Assert.False(line.HasScript);
      Assert.Equal(display.Ascent, line.Ascent);
      Assert.Equal(display.Descent, line.Descent);
      Assert.Equal(display.Width, line.Width);
      Assertions.ApproximatelyEqual(14, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(4, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(40, display.Width, 0.01);
    }

    [Fact]
    public void TestVariablesAndNumbers() {
      var mathList = MathLists.FromString("xy2w");

      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.NotNull(display);
      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 4), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var sub0 = display.Displays[0];
      var line = sub0 as TextLineDisplay<TFont, TGlyph>;
      Assert.NotNull(line);
      Assert.Equal(4, line.Atoms.Length);
      Assert.Equal("xy2w", line.StringText());
      Assert.Equal(new PointF(), line.Position);
      Assert.Equal(new Range(0, 4), line.Range);
      Assert.False(line.HasScript);

      Assert.Equal(display.Ascent, line.Ascent);
      Assert.Equal(display.Descent, line.Descent);
      Assert.Equal(display.Width, line.Width);
      Assertions.ApproximatelyEqual(14, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(4, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(40, display.Width, 0.01);
    }

    [Fact]
    public void TestSuperScript() {
      var mathList = new MathList();
      var x = MathAtoms.ForCharacter('x');
      var superscript = new MathList {
        MathAtoms.ForCharacter('2')
      };
      x.Superscript = superscript;
      mathList.Add(x);

      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.NotNull(display);

      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(display.IndexInParent, Range.UndefinedInt);
      Assert.Equal(2, display.Displays.Count());

      var super0 = display.Displays[0];
      var line = super0 as TextLineDisplay<TFont, TGlyph>;
      Assert.NotNull(line);
      Assert.Single(line.Atoms);
      Assert.Equal("x", line.StringText());
      Assert.Equal(new PointF(), line.Position);
      Assert.True(line.HasScript);

      var super1 = display.Displays[1] as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(super1);
      Assert.Equal(LinePosition.Supersript, super1.MyLinePosition);
      var super1Position = super1.Position;
      Assertions.ApproximatePoint(10.32, 7.26, super1Position, 0.01); // may change as we implement more details?
      Assert.Equal(new Range(0, 1), super1.Range);
      Assert.False(super1.HasScript);
      Assert.Equal(0, super1.IndexInParent);
      Assert.Single(super1.Displays);

      var super10 = super1.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.NotNull(super10);
      Assert.Single(super10.Atoms);
      Assert.Equal(new PointF(), super10.Position);
      Assert.False(super10.HasScript);

      Assertions.ApproximatelyEqual(17.06, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(4, display.Descent, 0.01);
    }

    [Fact]
    public void TestSubscript() {
      var mathList = new MathList();
      var x = MathAtoms.ForCharacter('x');
      var subscript = new MathList {
        MathAtoms.ForCharacter('1')
      };
      x.Subscript = subscript;
      mathList.Add(x);

      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.NotNull(display);

      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(display.IndexInParent, Range.UndefinedInt);
      Assert.Equal(2, display.Displays.Count());

      var sub0 = display.Displays[0];
      var line = sub0 as TextLineDisplay<TFont, TGlyph>;
      Assert.NotNull(line);
      Assert.Single(line.Atoms);
      Assert.Equal("x", line.StringText());
      Assert.Equal(new PointF(), line.Position);
      Assert.True(line.HasScript);
      var sub1 = display.Displays[1] as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(sub1);
      Assert.Equal(LinePosition.Subscript, sub1.MyLinePosition);
      var sub1Position = sub1.Position;
      Assertions.ApproximatePoint(10, -4.94, sub1Position, 0.01); // may change as we implement more details?
      Assert.Equal(new Range(0, 1), sub1.Range);
      Assert.False(sub1.HasScript);
      Assert.Equal(0, sub1.IndexInParent);
      Assert.Single(sub1.Displays);

      var sub10 = sub1.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.NotNull(sub10);
      Assert.Single(sub10.Atoms);
      Assert.Equal(new PointF(), sub10.Position);
      Assert.False(sub10.HasScript);

      Assertions.ApproximatelyEqual(14, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(7.74, display.Descent, 0.01);
    }

    [Fact]
    public void TestSuperSubscript() {
      var mathList = new MathList();
      var x = MathAtoms.ForCharacter('x');
      var superscript = new MathList {
        MathAtoms.ForCharacter('2')
      };
      var subscript = new MathList {
        MathAtoms.ForCharacter('1')
      };
      x.Subscript = subscript;
      x.Superscript = superscript;
      mathList.Add(x);

      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.NotNull(display);
      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Equal(3, display.Displays.Count());

      var line = display.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(line.Atoms);
      Assert.Equal("x", line.StringText());
      Assert.Equal(new PointF(), line.Position);
      Assert.True(line.HasScript);

      var display2 = display.Displays[1] as ListDisplay<TFont, TGlyph>;
      Assert.Equal(LinePosition.Supersript, display2.MyLinePosition);
      Assertions.ApproximatePoint(10.32, 9.68, display2.Position, 0.01);
      Assert.Equal(new Range(0, 1), display2.Range);
      Assert.False(display2.HasScript);
      Assert.Equal(0, display2.IndexInParent);
      Assert.Single(display2.Displays);

      var line2 = display2.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(line2.Atoms);
      Assert.Equal("2", line2.StringText());
      Assert.Equal(new PointF(), line2.Position);
      Assert.False(line2.HasScript);

      var display3 = display.Displays[2] as ListDisplay<TFont, TGlyph>;
      Assert.Equal(LinePosition.Subscript, display3.MyLinePosition);

      // Because both subscript and superscript are present, coords are
      // different from the subscript-only case.
      Assertions.ApproximatePoint(10, -6.12, display3.Position, 0.01);
      Assert.Equal(new Range(0, 1), display3.Range);
      Assert.False(display3.HasScript);
      Assert.Equal(0, display3.IndexInParent);
      Assert.Single(display3.Displays);

      var line3 = display3.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(line3.Atoms);
      Assert.Equal("1", line3.StringText());
      Assert.Equal(new PointF(), line3.Position);
      Assert.False(line3.HasScript);

      Assertions.ApproximatelyEqual(19.48, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(8.92, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(17.32, display.Width, 0.01);
      Assertions.ApproximatelyEqual(display.Ascent, display2.Position.Y + line2.Ascent, 0.01);
      Assertions.ApproximatelyEqual(display.Descent, line3.Descent - display3.Position.Y, 0.01);
    }
    [Fact]
    public void TestBinomial() {
      var list = new MathList();
      var fraction = new Fraction(false) {
        Numerator = new MathList {
          MathAtoms.ForCharacter('1')
        },
        Denominator = new MathList {
          MathAtoms.ForCharacter('3')
        },
        LeftDelimiter = "(",
        RightDelimiter = ")"
      };
      list.Add(fraction);

      var display = Typesetter<TFont, TGlyph>.CreateLine(list, _font, _context, LineStyle.Display);
      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var display0 = display.Displays[0] as ListDisplay<TFont, TGlyph>;
      Assert.Equal(LinePosition.Regular, display0.MyLinePosition);
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
      Assertions.ApproximatePoint(10, 0, subFraction.Position, 0.01);
      
      var numerator = subFraction.Numerator as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(numerator);
      Assert.Equal(LinePosition.Regular, numerator.MyLinePosition);
      Assertions.ApproximatePoint(10, 13.54, numerator.Position, 0.01);
      Assert.Single(numerator.Displays);
      Assert.Equal(Range.UndefinedInt, numerator.IndexInParent);
      Assert.False(numerator.HasScript);

      var subNumerator = numerator.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(subNumerator.Atoms);
      Assert.Equal("1", subNumerator.StringText());
      Assert.Equal(new PointF(), subNumerator.Position);
      Assert.Equal(new Range(0, 1), subNumerator.Range);
      Assert.False(subNumerator.HasScript);

      var denominator = subFraction.Denominator as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(denominator);
      Assert.Equal(LinePosition.Regular, denominator.MyLinePosition);
      Assertions.ApproximatePoint(10, -13.72, denominator.Position, 0.01);
      Assert.Equal(new Range(0, 1), denominator.Range);
      Assert.False(denominator.HasScript);
      Assert.Equal(Range.UndefinedInt, denominator.IndexInParent);
      Assert.Single(denominator.Displays);

      var subDenominator = denominator.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(subDenominator.Atoms);
      Assert.Equal("3", subDenominator.StringText());
      Assert.Equal(new PointF(), subDenominator.Position);
      Assert.Equal(new Range(0, 1), subDenominator.Range);
      Assert.False(subDenominator.HasScript);

      var subRight = display0.Displays[2] as GlyphDisplay<TFont, TGlyph>;
      Assert.False(subRight.HasScript);
      Assert.Equal(Range.NotFound, subRight.Range);
      Assertions.ApproximatePoint(20, 0, subRight.Position, 0.01);
      Assertions.ApproximatelyEqual(27.54, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(17.72, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(30, display.Width, 0.01);
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
      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
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
      Assert.Equal(LinePosition.Regular, numerator.MyLinePosition);
      Assertions.ApproximatePoint(0, 13.54, numerator.Position, 0.01);
      Assert.Equal(new Range(0, 1), numerator.Range);
      Assert.False(numerator.HasScript);
      Assert.Equal(Range.UndefinedInt, numerator.IndexInParent);
      Assert.Single(numerator.Displays);

      var subNumerator = numerator.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(subNumerator.Atoms);
      Assert.Equal("1", subNumerator.StringText());
      Assert.Equal(new PointF(), subNumerator.Position);
      Assert.Equal(new Range(0, 1), subNumerator.Range);
      Assert.False(subNumerator.HasScript);

      var denominator = fraction.Denominator as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(denominator);
      Assert.Equal(LinePosition.Regular, denominator.MyLinePosition);
      Assertions.ApproximatePoint(0, -13.72, denominator.Position, 0.01);
      Assert.Equal(new Range(0, 1), denominator.Range);
      Assert.False(denominator.HasScript);
      Assert.Equal(Range.UndefinedInt, denominator.IndexInParent);
      Assert.Single(denominator.Displays);

      var subDenominator = denominator.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(subDenominator.Atoms);
      Assert.Equal("3", subDenominator.StringText());
      Assert.Equal(new PointF(), subDenominator.Position);
      Assert.Equal(new Range(0, 1), subDenominator.Range);
      Assert.False(subDenominator.HasScript);

      Assertions.ApproximatelyEqual(27.54, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(17.72, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(10, display.Width, 0.01);
    }

    [Fact]
    public void TestEquationWithOperatorsAndRelations() {
      var mathList = MathLists.FromString("2x+3=y");
      var display = Typesetter<TFont, TGlyph>.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new Range(0, 6), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var line = display.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Equal(6, line.Atoms.Length);
      Assert.Equal("2x+3=y", line.StringText());
      Assert.Equal(new PointF(), line.Position);
      Assert.Equal(new Range(0, 6), line.Range);
      Assert.False(line.HasScript);

      Assert.Equal(display.Ascent, line.Ascent);
      Assert.Equal(display.Width, line.Width);
      Assert.Equal(display.Descent, line.Descent);

      Assertions.ApproximatelyEqual(14, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(4, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(80, display.Width, 0.01);
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
      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
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
      Assert.Equal(LinePosition.Regular, numerator.MyLinePosition);
      Assert.False(numerator.HasScript);
      Assertions.ApproximatePoint(0, 13.54, numerator.Position, 0.01);
      Assert.Equal(new Range(0, 1), numerator.Range);
      Assert.Equal(Range.UndefinedInt, numerator.IndexInParent);
      Assert.Single(numerator.Displays);

      var subNumerator = numerator.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(subNumerator.Atoms);
      Assert.Equal("1", subNumerator.StringText());
      Assert.Equal(new PointF(), subNumerator.Position);
      Assert.Equal(new Range(0, 1), subNumerator.Range);
      Assert.False(subNumerator.HasScript);

      var denominator = fraction.Denominator as ListDisplay<TFont, TGlyph>;
      Assert.NotNull(denominator);
      Assert.Equal(LinePosition.Regular, denominator.MyLinePosition);
      Assertions.ApproximatePoint(0, -13.73, denominator.Position, 0.01);
      Assert.Equal(new Range(0, 1), denominator.Range);
      Assert.False(denominator.HasScript);
      Assert.Equal(Range.UndefinedInt, denominator.IndexInParent);
      Assert.Single(denominator.Displays);

      var subDenominator = denominator.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(subDenominator.Atoms);
      Assert.Equal("3", subDenominator.StringText());
      Assert.Equal(new PointF(), subDenominator.Position);
      Assert.Equal(new Range(0, 1), subDenominator.Range);
      Assert.False(subDenominator.HasScript);

      Assertions.ApproximatelyEqual(27.54, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(17.72, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(10, display.Width, 0.01);
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
      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var display2 = display.Displays[0] as ListDisplay<TFont, TGlyph>;
      Assert.Equal(LinePosition.Regular, display2.MyLinePosition);
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
      Assert.Equal(LinePosition.Regular, display3.MyLinePosition);
      Assertions.ApproximatePoint(10, 0, display3.Position, 0.01);
      Assert.Equal(new Range(0, 1), display3.Range);
      Assert.False(display3.HasScript);
      Assert.Equal(Range.UndefinedInt, display3.IndexInParent);
      Assert.Single(display3.Displays);

      var line = display3.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(line.Atoms);
      Assert.Equal("x", line.StringText());
      Assert.Equal(new PointF(), line.Position);
      Assert.False(line.HasScript);

      var glyph2 = display2.Displays[2] as GlyphDisplay<TFont, TGlyph>;
      Assertions.ApproximatePoint(20, 0, glyph2.Position, 0.01);
      Assert.Equal(Range.NotFound, glyph2.Range);
      Assert.False(glyph2.HasScript);

      Assert.Equal(display.Ascent, display2.Ascent);
      Assert.Equal(display.Descent, display2.Descent);
      Assert.Equal(display.Width, display2.Width);

      Assertions.ApproximatelyEqual(14, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(4, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(30, display.Width, 0.01);
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
      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
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
      Assert.Equal(LinePosition.Regular, display2.MyLinePosition);
      Assertions.ApproximatePoint(10, 0, display2.Position, 0.01);
      Assert.Equal(new Range(0, 1), display2.Range);
      Assert.False(display2.HasScript);
      Assert.Equal(Range.UndefinedInt, display2.IndexInParent);
      Assert.Single(display2.Displays);

      var line2 = display2.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(line2.Atoms);
      Assert.Equal("1", line2.StringText());
      Assert.Equal(new PointF(), line2.Position);
      Assert.Equal(new Range(0, 1), line2.Range);
      Assert.False(line2.HasScript);

      Assertions.ApproximatelyEqual(18.56, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(4, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(20, display.Width, 0.01);
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
      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var display2 = display.Displays[0] as ListDisplay<TFont, TGlyph>;
      Assert.Equal(LinePosition.Regular, display2.MyLinePosition);
      Assert.Equal(new PointF(0, 3), display2.Position);
      Assert.Equal(new Range(0, 1), display2.Range);
      Assert.False(display2.HasScript);
      Assert.Equal(Range.UndefinedInt, display2.IndexInParent);
      Assert.Equal(1, display2.Displays.Count);
      
      var line = display2.Displays[0] as TextLineDisplay<TFont, TGlyph>;
      Assert.Single(line.Atoms);
      Assert.Equal("r", line.StringText());
      Assert.Equal(new PointF(), line.Position);
      Assert.False(line.HasScript);

      Assertions.ApproximatelyEqual(17, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(1, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(10, display.Width, 0.01);
    }
  }
}
