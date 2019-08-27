using CSharpMath.Display;
using CSharpMath.Tests.FrontEnd;
using System;
using System.Drawing;
using Xunit;
using TGlyph = System.Char;
using CSharpMath.Display.Text;
using System.Linq;

namespace CSharpMath.Tests.Mocks {
  // purpose of this class is to make sure our mocks behave as expected.
  public class MockTests {
    [Fact]
    public void TestGlyphBoundsWithoutM() {
      string hello = "Hello";
      TestFont font = new TestFont(10);
      var provider = TestGlyphBoundsProvider.Instance;
      var glyphRun = new AttributedGlyphRun<TestFont, TGlyph>(hello, hello, font);
      var width = provider.GetTypographicWidth(font, glyphRun);
      Assertions.ApproximatelyEqual(width, 25,  0.01);
    }

    [Fact]
    public void TestGlyphBoundsWithM() {
      string america = "America";
      TestFont font = new TestFont(10);
      var provider = TestGlyphBoundsProvider.Instance;
      var glyphRun = new AttributedGlyphRun<TestFont, TGlyph>(america, america, font);
      var width = provider.GetTypographicWidth(font, glyphRun);
      Assertions.ApproximatelyEqual(width, 40, 0.01);
    }
  }
}
