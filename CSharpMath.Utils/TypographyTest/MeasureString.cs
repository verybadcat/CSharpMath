using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.DevUtils.TypographyTest {
  static class MeasureString {
    /// <summary>
    /// Tests the Typography library for correctly measuring the string's width.
    /// </summary>
    /// <param name="text">The string to measure.</param>
    /// <returns>The width of the string.</returns>
    public static float Measure(string text) => new Typography.TextLayout.GlyphLayout() {
      Typeface = new Typography.OpenFont.OpenFontReader().Read(new System.IO.FileStream(Paths.LatinModernMath, System.IO.FileMode.Open))
    }.LayoutAndMeasureString(text.ToCharArray(), 0, text.Length, 20, out _).width;
  }
}
