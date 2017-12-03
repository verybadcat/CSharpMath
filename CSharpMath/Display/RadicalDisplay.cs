using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.Display.Text;

namespace CSharpMath.Display {
  public class RadicalDisplay : IDisplay {
    // A display representing the numerator of the fraction. Its position is relative
    // to the parent and it is not treated as a sub-display.
    public MathListDisplay Radicand { get; private set; }
    // A display representing the numerator of the fraction. Its position is relative
    // to the parent and it is not treated as a sub-display.
    public MathListDisplay Degree { get; private set; }

    public float Width { get; set; }

    private float _radicalShift;
    private IDisplay _radicalGlyph;

    public void SetDegree(MathListDisplay degree, MathFont degreeFont, FontMathTable degreeFontMathTable) {
      var kernBefore = degreeFontMathTable.RadicalKernBeforeDegree(degreeFont);
      var kernAfter = degreeFontMathTable.RadicalKernAfterDegree(degreeFont);
      var raise = degreeFontMathTable.RadicalDegreeBottomRaisePercent(degreeFont) * (this.Ascent - this.Descent);
      Degree = degree;
      _radicalShift = kernBefore + degree.Width + kernAfter;
      if (_radicalShift < 0) {
        kernBefore -= _radicalShift;
        _radicalShift = 0;
      }



      // Position of degree is relative to parent.
      Degree.Position = new PointF(this.Position.X + kernBefore, this.Position.Y + raise);
      // update the width by the _radicalShift
      Width = _radicalShift + _radicalGlyph.Width + Radicand.Width;
      _UpdateRadicandPosition();
    }

    private void _UpdateRadicandPosition() {
      var x = this.Position.X + _radicalShift + _radicalGlyph.Width;
      Radicand.Position = new PointF(x, this.Position.Y);
    }

    public RectangleF DisplayBounds => throw new NotImplementedException();

    public float Ascent => throw new NotImplementedException();

    public float Descent => throw new NotImplementedException();



    public Range Range => throw new NotImplementedException();

    public PointF Position => throw new NotImplementedException();
    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext context) => throw new NotImplementedException();
  }
}
