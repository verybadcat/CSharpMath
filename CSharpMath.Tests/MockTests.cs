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
      MathFont<TGlyph> font = new MathFont<TGlyph>(10);
      var provider = new TestGlyphBoundsProvider();
      var glyphRun = new AttributedGlyphRun<MathFont<TGlyph>, TGlyph>
      {
        Font = font,
        KernedGlyphs = hello.ToCharArray().Select(c => new KernedGlyph<char>(c)).ToArray(),
      };
      var width = provider.GetTypographicWidth(font, glyphRun);
      Assertions.ApproximatelyEqual(width, 25,  0.01);
    }

    [Fact]
    public void TestGlyphBoundsWithM() {
      string america = "America";
      MathFont<TGlyph> font = new MathFont<TGlyph>(10);
      var provider = new TestGlyphBoundsProvider();
      var glyphRun = new AttributedGlyphRun<MathFont<TGlyph>, TGlyph>
      {
        Font = font,
        KernedGlyphs = america.ToCharArray().Select(c => new KernedGlyph<char>(c)).ToArray(),
      };
      var width = provider.GetTypographicWidth(font, glyphRun);
      Assertions.ApproximatelyEqual(width, 40, 0.01);
    }
  }
}
