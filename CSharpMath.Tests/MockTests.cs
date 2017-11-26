using CSharpMath.Display.Text;
using CSharpMath.Tests.FrontEnd;
using System;
using System.Drawing;
using Xunit;

namespace CSharpMath.Tests {
  // purpose of this class is to make sure our mocks behave as expected.
  public class MockTests {
    [Fact]
    public void TestGlyphBoundsWithoutM() {
      string hello = "Hello";
      MathFont font = new MathFont(10);
      var provider = new TestGlyphBoundsProvider();
      RectangleF bounds = provider.GetBoundingRectForGlyphs(font, hello);
      Assertions.ApproximatelyEquals(bounds, 0, -2, 5, 2.5,  0.01);
    }

    [Fact]
    public void TestGlyphBoundsWithM() {
      string america = "America";
      MathFont font = new MathFont(10);
      var provider = new TestGlyphBoundsProvider();
      RectangleF bounds = provider.GetBoundingRectForGlyphs(font, america);
      Assertions.ApproximatelyEquals(bounds, 0, -2, 8, 2.5, 0.01);
    }
  }
}
