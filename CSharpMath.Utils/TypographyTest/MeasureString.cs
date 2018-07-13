using System;
using System.Collections.Generic;
using System.Text;
using static Typography.TextLayout.PixelScaleLayoutExtensions;

namespace CSharpMath.DevUtils.TypographyTest {
  static class MeasureString {
    /// <summary>
    /// Tests the Typography library for correctly measuring the string's width.
    /// </summary>
    /// <param name="text">The string to measure.</param>
    /// <returns>The width of the string.</returns>
    public static float Measure(string text, float pointSize = 20) => _Statics.GlyphLayout.LayoutAndMeasureString(text.ToCharArray(), 0, text.Length, pointSize).width;
  }
}
