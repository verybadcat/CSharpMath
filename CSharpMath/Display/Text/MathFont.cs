using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display.Text {
  public class MathFont {
    public MathFont(float pointSize) {
      PointSize = pointSize;
    }
    public MathFont(float pointSize, FontStyle style) {
      PointSize = pointSize;
      Style = style;
    }
    internal FontMathTable MathTable { get; }
    public float PointSize { get; }
    public FontStyle Style { get; }
    public MathFont CopyWithSize(float pointSize)
      => new MathFont(pointSize, Style);
    public bool Equals(MathFont otherFont) =>
      PointSize.Equals(otherFont.PointSize)
      && Style.Equals(otherFont.Style);
    public override bool Equals(object obj) {
      if (obj is MathFont font) {
        return Equals(font);
      }
      return false;
    }
    public override int GetHashCode() {
      unchecked {
        return Style.GetHashCode() + 13 * PointSize.GetHashCode();
      }
    }
  }
}
