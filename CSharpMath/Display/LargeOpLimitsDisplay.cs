using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;

namespace CSharpMath.Display {
  public class LargeOpLimitsDisplay<TFont, TGlyph> : IPositionableDisplay<TFont, TGlyph>
    where TFont : MathFont<TGlyph> {
    private IPositionableDisplay<TFont, TGlyph> _nucleusDisplay;
    private MathListDisplay<TFont, TGlyph> _upperLimit;
    private MathListDisplay<TFont, TGlyph> _lowerLimit;
    private float _limitShift;
    private int _extraPadding;
    private float _upperLimitGap { get; set; }
    private float _lowerLimitGap { get; set; }

    public LargeOpLimitsDisplay(IPositionableDisplay<TFont, TGlyph> nucleusDisplay, MathListDisplay<TFont, TGlyph> upperLimit, MathListDisplay<TFont, TGlyph> lowerLimit, float limitShift, int extraPadding) {
      _nucleusDisplay = nucleusDisplay;
      _upperLimit = upperLimit;
      _lowerLimit = lowerLimit;
      _limitShift = limitShift;
      _extraPadding = extraPadding; // corresponds to \xi_13 in TeX.
      var upperWidth = upperLimit?.Width ?? 0f;
      var lowerWidth = lowerLimit?.Width ?? 0f;
      var nucleusWidth = nucleusDisplay?.Width ?? 0f;
      var maxWidth = Math.Max(nucleusWidth, Math.Max(upperWidth, lowerWidth));
      Width = maxWidth;
    }

    // A display representing the numerator of the fraction. Its position is relative
    // to the parent and it is not treated as a sub-display.
    public MathListDisplay<TFont, TGlyph> UpperLimit { get; private set; }
    // A display representing the numerator of the fraction. Its position is relative
    // to the parent and it is not treated as a sub-display.
    public MathListDisplay<TFont, TGlyph> LowerLimit { get; private set; }

    public RectangleF DisplayBounds => this.ComputeDisplayBounds();

    public float Ascent {
      get {
        if (UpperLimit == null) {
          return _nucleusDisplay.Ascent;
        }
        return _nucleusDisplay.Ascent + _extraPadding + _upperLimit.Ascent
          + _upperLimitGap + _upperLimit.Descent;
      }
    }

    public float Descent {
      get {
        if (LowerLimit == null) {
          return _nucleusDisplay.Descent;
        }
        return _nucleusDisplay.Descent + _extraPadding + _lowerLimit.Descent
          + _lowerLimitGap + _lowerLimit.Descent;
      }
    }


    public float Width { get; set; }

    public Range Range {
      get {
        var r = _nucleusDisplay.Range;
        if (_upperLimit!=null) {
          r = RangeExtensions.Combine(r, _upperLimit.Range);
        }
        if (_lowerLimit!=null) {
          r = RangeExtensions.Combine(r, _lowerLimit.Range);
        }
        return r;
      }
    }

    public PointF Position { get; set; }
    public void SetPosition(PointF position) {
      Position = position;
    }
    public bool HasScript { get; set; }
    private void _UpdateLowerLimitPosition() {
      if (LowerLimit!=null) {
        LowerLimit.Position = new PointF(
          Position.X - _limitShift + (Width - _lowerLimit.Width) / 2,
          Position.Y - _nucleusDisplay.Descent - _lowerLimitGap - _lowerLimit.Ascent);
      }
    }
    private void _UpdateUpperLimitPosition() {
      if (UpperLimit!=null) {
        UpperLimit.Position = new PointF(
          Position.X + _limitShift + (Width - UpperLimit.Width / 2),
          Position.Y + _nucleusDisplay.Ascent + _upperLimitGap + UpperLimit.Descent);
      }
    }
    private void _UpdateNucleusPosition() {
      var position = new PointF(
        Position.X + (Width - _nucleusDisplay.Width) / 2,
        Position.Y);
      _nucleusDisplay.SetPosition(position);
    }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      UpperLimit?.Draw(context);
      LowerLimit?.Draw(context);
      _nucleusDisplay.Draw(context);
    }
  }
}
