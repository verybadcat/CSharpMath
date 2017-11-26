using CSharpMath.Atoms;
using CSharpMath.Display;
using CSharpMath.Display.Text;
using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using CSharpMath.Tests.FrontEnd;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CSharpMath.Tests {
  public class TypesettingTests {
    public TypesettingTests() {
      
    }
    private TypesettingContext _context => TestTypesettingContexts.Create();
    [Fact]
    public void TestSimpleVariable() {
      var list = new MathList {
        MathAtoms.ForCharacter('x')
      };
      var display = Typesetter.CreateLine(list, _font, LineStyle.Display);
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
      Assert.Single(line.Atoms);

      Assert.Equal("x", line.AttributedText.Text);
      Assert.Equal(new PointF(), line.Position);
      Assert.Equal(new Range(0, 1), line.Range);
      Assert.False(line.HasScript);

      Assertions.ApproximatelyEquals(2, display.Ascent, 0.01);
      Assertions.ApproximatelyEquals(0.5, display.Descent, 0.01);
      Assertions.ApproximatelyEquals(1, display.Width, 0.01);
      Assert.Equal(display.Ascent, line.Ascent);
      Assert.Equal(display.Descent, line.Descent);
      Assert.Equal(display.Width, line.Width);
    }
  }
}
