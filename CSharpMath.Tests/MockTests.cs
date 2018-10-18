using CSharpMath.Display;
using CSharpMath.Tests.FrontEnd;
using System;
using System.Drawing;
using Xunit;
using TGlyph = System.Char;
using CSharpMath.Display.Text;
using System.Linq;

namespace CSharpMath.Tests {
  // purpose of this class is to make sure our mocks behave as expected.
  public class MockTests {
    [Fact]
    public void TestGlyphBoundsWithoutM() {
      string hello = "Hello";
      TestMathFont font = new TestMathFont(10);
      var provider = new TestGlyphBoundsProvider();
      var glyphRun = new AttributedGlyphRun<TestMathFont, TGlyph>(hello, hello, font);
      var width = provider.GetTypographicWidth(font, glyphRun);
      Assertions.ApproximatelyEqual(width, 25,  0.01);
    }

    [Fact]
    public void TestGlyphBoundsWithM() {
      string america = "America";
      TestMathFont font = new TestMathFont(10);
      var provider = new TestGlyphBoundsProvider();
      var glyphRun = new AttributedGlyphRun<TestMathFont, TGlyph>(america, america, font);
      var width = provider.GetTypographicWidth(font, glyphRun);
      Assertions.ApproximatelyEqual(width, 40, 0.01);
    }
  }
}
