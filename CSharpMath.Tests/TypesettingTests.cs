using CSharpMath.Atoms;
using CSharpMath.Display;
using CSharpMath.Display.Text;
using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using CSharpMath.Interfaces;
using CSharpMath.Tests.FrontEnd;
using System.Drawing;
using Xunit;

namespace CSharpMath.Tests {
  public class TypesettingTests {
    public TypesettingTests() {
      
    }
    private MathFont _font { get; } = new MathFont(10);
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

      Assertions.ApproximatelyEqual(2, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(0.5, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(1, display.Width, 0.01);
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
      Assert.Equal(Range.Undefined, display.IndexInParent);
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
      Assertions.ApproximatelyEqual(2, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(0.5, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(4, display.Width, 0.01);
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
      Assert.Equal(Range.Undefined, display.IndexInParent);
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
      Assertions.ApproximatelyEqual(2, display.Ascent, 0.01);
      Assertions.ApproximatelyEqual(0.5, display.Descent, 0.01);
      Assertions.ApproximatelyEqual(4, display.Width, 0.01);
    }
  }
}
