using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display {
  public class MathFont<TGlyph> {
    public MathFont(float pointSize) {
      PointSize = pointSize;
    }
    public MathFont(float pointSize, FontStyle style) {
      PointSize = pointSize;
      Style = style;
    }
    public float PointSize { get; }
    public FontStyle Style { get; }
    public bool Equals(MathFont<TGlyph> otherFont) =>
      PointSize.Equals(otherFont.PointSize)
      && Style.Equals(otherFont.Style);
    public override bool Equals(object obj) {
      if (obj is MathFont<TGlyph> font) {
        return Equals(font);
      }
      return false;
    }
    public override int GetHashCode() {
      unchecked {
        return Style.GetHashCode() + 13 * PointSize.GetHashCode();
      }
    }

    public override string ToString() => $"[{nameof(MathFont<TGlyph>)}<{typeof(TGlyph).Name}>: {nameof(PointSize)}={PointSize}, {nameof(Style)}={Style}]";
  }
}
