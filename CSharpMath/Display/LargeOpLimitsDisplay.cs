using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.FrontEnd;
using Color = CSharpMath.Structures.Color;

namespace CSharpMath.Display {
  public class LargeOpLimitsDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : MathFont<TGlyph> {
    private IDisplay<TFont, TGlyph> _nucleusDisplay;
    private readonly float _limitShift;
    private readonly int _extraPadding;
    private float _upperLimitGap;
    private float _lowerLimitGap;

    public void SetUpperLimitGap(float value) {
      _upperLimitGap = value;
      _UpdateUpperLimitPosition();
    }

    public void SetLowerLimitGap(float value) {
      _lowerLimitGap = value;
      _UpdateLowerLimitPosition();
    }

    public LargeOpLimitsDisplay(IDisplay<TFont, TGlyph> nucleusDisplay, IDisplay<TFont, TGlyph> upperLimit, IDisplay<TFont, TGlyph> lowerLimit, float limitShift, int extraPadding) {
      _nucleusDisplay = nucleusDisplay;
      UpperLimit = upperLimit;
      LowerLimit = lowerLimit;
      _limitShift = limitShift;
      _extraPadding = extraPadding; // corresponds to \xi_13 in TeX.
      var upperWidth = upperLimit?.Width ?? 0f;
      var lowerWidth = lowerLimit?.Width ?? 0f;
      var nucleusWidth = nucleusDisplay?.Width ?? 0f;
      var maxWidth = Math.Max(nucleusWidth, Math.Max(upperWidth, lowerWidth));
      Width = maxWidth;
      _UpdateComponentPositions();
    }

    // A display representing the numerator of the fraction. Its position is relative
    // to the parent and it is not treated as a sub-display.
    public IDisplay<TFont, TGlyph> UpperLimit { get; private set; }
    // A display representing the numerator of the fraction. Its position is relative
    // to the parent and it is not treated as a sub-display.
    public IDisplay<TFont, TGlyph> LowerLimit { get; private set; }

    public RectangleF DisplayBounds => this.ComputeDisplayBounds();

    public float Ascent {
      get {
        if (UpperLimit == null) {
          return _nucleusDisplay.Ascent;
        }
        return _nucleusDisplay.Ascent + _extraPadding + UpperLimit.Ascent
          + _upperLimitGap + UpperLimit.Descent;
      }
    }

    public float Descent {
      get {
        if (LowerLimit == null) {
          return _nucleusDisplay.Descent;
        }
        return _nucleusDisplay.Descent + _extraPadding + LowerLimit.Descent
          + _lowerLimitGap + LowerLimit.Descent;
      }
    }


    public float Width { get; set; }

    public Range Range {
      get {
        var r = _nucleusDisplay.Range;
        if (UpperLimit!=null) {
          r = RangeExtensions.Combine(r, UpperLimit.Range);
        }
        if (LowerLimit!=null) {
          r = RangeExtensions.Combine(r, LowerLimit.Range);
        }
        return r;
      }
    }

    PointF _position;
    public PointF Position { get => _position; set { _position = value; _UpdateComponentPositions(); } }

    public bool HasScript { get; set; }

    private void _UpdateComponentPositions() {
      _UpdateNucleusPosition();
      _UpdateUpperLimitPosition();
      _UpdateLowerLimitPosition();
    }
    private void _UpdateLowerLimitPosition() {
      if (LowerLimit!=null) {
        LowerLimit.Position = new PointF(
          Position.X - _limitShift + (Width - LowerLimit.Width) / 2,
          Position.Y - _nucleusDisplay.Descent - _lowerLimitGap - LowerLimit.Ascent);
      }
    }
    private void _UpdateUpperLimitPosition() {
      if (UpperLimit!=null) {
        UpperLimit.Position = new PointF(
          Position.X + _limitShift + (Width - UpperLimit.Width) / 2,
          Position.Y + _nucleusDisplay.Ascent + _upperLimitGap + UpperLimit.Descent);
      }
    }
    private void _UpdateNucleusPosition() {
      var position = new PointF(
        Position.X + (Width - _nucleusDisplay.Width) / 2,
        Position.Y);
      _nucleusDisplay.Position = position;
    }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      UpperLimit?.Draw(context);
      LowerLimit?.Draw(context);
      _nucleusDisplay.Draw(context);
    }

    public Color? TextColor { get; set; }

    public void SetTextColorRecursive(Color? textColor) {
      TextColor = TextColor ?? textColor;
      UpperLimit.SetTextColorRecursive(textColor);
      LowerLimit.SetTextColorRecursive(textColor);
    }
  }
}
