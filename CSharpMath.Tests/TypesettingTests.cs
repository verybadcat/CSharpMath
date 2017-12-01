using CSharpMath.Atoms;
using CSharpMath.Display;
using CSharpMath.Display.Text;
using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using CSharpMath.Tests.FrontEnd;
using System.Drawing;
using System.Linq;
using Xunit;

namespace CSharpMath.Tests {
  public class TypesettingTests {
    public TypesettingTests() {
      
    }
    private MathFont _font { get; } = new MathFont(20);
    private IFontMeasurer _fontMeasurer => _context.FontMeasurer;
    private TypesettingContext _context { get; } = TestTypesettingContexts.Create();
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
      Assert.True(sub0 is TextLineDisplay);
      var line = sub0 as TextLineDisplay;
      //Assert.Single(line.Atoms); // have to think about these; doesn't really work atm

      Assert.Equal("x", line.Text);
      Assert.Equal(new PointF(), line.Position);
      Assert.Equal(new Range(0, 1), line.Range);
      Assert.False(line.HasScript);

      Assertions.ApproximatelyEqual(14, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(4, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(10, display.Width, 0.01);
      Assert.Equal(display.Ascent, line.Ascent);
      Assert.Equal(display.Descent, line.Descent);
      Assert.Equal(display.Width, line.Width);
    }

    [Fact]
    public void TestMultipleVariables() {
      var list = MathLists.FromString("xyzw");
      var display = Typesetter.CreateLine(list, _font, _context, LineStyle.Display);

      Assert.NotNull(display);

      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 4), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var subDisplay = display.Displays[0];
      var line = subDisplay as TextLineDisplay;
      Assert.NotNull(line);
      Assert.Equal(4, line.Atoms.Count);

      Assert.Equal("xyzw", line.Text);
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

      var display = Typesetter.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.NotNull(display);
      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 4), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var sub0 = display.Displays[0];
      var line = sub0 as TextLineDisplay;
      Assert.NotNull(line);
      Assert.Equal(4, line.Atoms.Count);
      Assert.Equal("xy2w", line.Text);
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
      var superscript = new MathList();
      superscript.Add(MathAtoms.ForCharacter('2'));
      x.Superscript = superscript;
      mathList.Add(x);

      var display = Typesetter.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.NotNull(display);

      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(display.IndexInParent, Range.UndefinedInt);
      Assert.Equal(2, display.Displays.Count());

      var super0 = display.Displays[0];
      var line = super0 as TextLineDisplay;
      Assert.NotNull(line);
      Assert.Single(line.Atoms);
      Assert.Equal("x", line.Text);
      Assert.Equal(new PointF(), line.Position);
      Assert.True(line.HasScript);

      var super1 = display.Displays[1] as MathListDisplay;
      Assert.NotNull(super1);
      Assert.Equal(LinePosition.Supersript, super1.MyLinePosition);
      var super1Position = super1.Position;
      Assertions.ApproximatelyEqual(10, 7.26, super1Position, 0.01); // may change as we implement more details?
      Assert.Equal(new Range(0, 1), super1.Range);
      Assert.False(super1.HasScript);
      Assert.Equal(0, super1.IndexInParent);
      Assert.Single(super1.Displays);

      var super10 = super1.Displays[0] as TextLineDisplay;
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
      var subscript = new MathList();
      subscript.Add(MathAtoms.ForCharacter('1'));
      x.Subscript = subscript;
      mathList.Add(x);

      var display = Typesetter.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.NotNull(display);

      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(display.IndexInParent, Range.UndefinedInt);
      Assert.Equal(2, display.Displays.Count());

      var sub0 = display.Displays[0];
      var line = sub0 as TextLineDisplay;
      Assert.NotNull(line);
      Assert.Single(line.Atoms);
      Assert.Equal("x", line.Text);
      Assert.Equal(new PointF(), line.Position);
      Assert.True(line.HasScript);
      var sub1 = display.Displays[1] as MathListDisplay;
      Assert.NotNull(sub1);
      Assert.Equal(LinePosition.Subscript, sub1.MyLinePosition);
      var sub1Position = sub1.Position;
      Assertions.ApproximatelyEqual(10, -4.94, sub1Position, 0.01); // may change as we implement more details?
      Assert.Equal(new Range(0, 1), sub1.Range);
      Assert.False(sub1.HasScript);
      Assert.Equal(0, sub1.IndexInParent);
      Assert.Single(sub1.Displays);

      var sub10 = sub1.Displays[0] as TextLineDisplay;
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

      var display = Typesetter.CreateLine(mathList, _font, _context, LineStyle.Display);
      Assert.NotNull(display);
      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Equal(3, display.Displays.Count());

      var line = display.Displays[0] as TextLineDisplay;
      Assert.Single(line.Atoms);
      Assert.Equal("x", line.Text);
      Assert.Equal(new PointF(), line.Position);
      Assert.True(line.HasScript);

      var display2 = display.Displays[1] as MathListDisplay;
      Assert.Equal(LinePosition.Supersript, display2.MyLinePosition);
      Assertions.ApproximatelyEqual(10, 9.68, display2.Position, 0.01);
      Assert.Equal(new Range(0, 1), display2.Range);
      Assert.False(display2.HasScript);
      Assert.Equal(0, display2.IndexInParent);
      Assert.Single(display2.Displays);

      var line2 = display2.Displays[0] as TextLineDisplay;
      Assert.Single(line2.Atoms);
      Assert.Equal("2", line2.Text);
      Assert.Equal(new PointF(), line2.Position);
      Assert.False(line2.HasScript);

      var display3 = display.Displays[2] as MathListDisplay;
      Assert.Equal(LinePosition.Subscript, display3.MyLinePosition);

      // Because both subscript and superscript are present, coords are
      // different from the subscript-only case.
      Assertions.ApproximatelyEqual(10, -6.12, display3.Position, 0.01);
      Assert.Equal(new Range(0, 1), display3.Range);
      Assert.False(display3.HasScript);
      Assert.Equal(0, display3.IndexInParent);
      Assert.Single(display3.Displays);

      var line3 = display3.Displays[0] as TextLineDisplay;
      Assert.Single(line3.Atoms);
      Assert.Equal("1", line3.Text);
      Assert.Equal(new PointF(), line3.Position);
      Assert.False(line3.HasScript);

      Assertions.ApproximatelyEqual(19.48, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(8.92, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(17, display.Width, 0.01);
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

      var display = Typesetter.CreateLine(list, _font, _context, LineStyle.Display);
      Assert.Equal(LinePosition.Regular, display.MyLinePosition);
      Assert.Equal(new PointF(), display.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display.HasScript);
      Assert.Equal(Range.UndefinedInt, display.IndexInParent);
      Assert.Single(display.Displays);

      var display0 = display.Displays[0] as MathListDisplay;
      Assert.Equal(LinePosition.Regular, display0.MyLinePosition);
      Assert.Equal(new PointF(), display0.Position);
      Assert.Equal(new Range(0, 1), display.Range);
      Assert.False(display0.HasScript);
      Assert.Equal(Range.UndefinedInt, display0.IndexInParent);
      Assert.Equal(3, display0.Displays.Count());
    }
  }
}
