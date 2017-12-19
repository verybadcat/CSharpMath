using CSharpMath.Display;
using CSharpMath.Tests.FrontEnd;
using System;
using System.Drawing;
using Xunit;
using TGlyph = System.Char;

namespace CSharpMath.Tests {
  // purpose of this class is to make sure our mocks behave as expected.
  public class MockTests {
    [Fact]
    public void TestGlyphBoundsWithoutM() {
      string hello = "Hello";
      MathFont<TGlyph> font = new MathFont<TGlyph>(10);
      var provider = new TestGlyphBoundsProvider();
      RectangleF bounds = provider.GetCombinedBoundingRectForGlyphs(font, hello.ToCharArray());
      Assertions.ApproximatelyEquals(bounds, 0, -2, 25, 9,  0.01);
    }

    [Fact]
    public void TestGlyphBoundsWithM() {
      string america = "America";
      MathFont<TGlyph> font = new MathFont<TGlyph>(10);
      var provider = new TestGlyphBoundsProvider();
      RectangleF bounds = provider.GetCombinedBoundingRectForGlyphs(font, america.ToCharArray());
      Assertions.ApproximatelyEquals(bounds, 0, -2, 40, 9, 0.01);
    }
  }
}
