using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.FrontEnd;
using Color = CSharpMath.Structures.Color;

namespace CSharpMath.Display {
  public class LargeOpLimitsDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    private readonly IDisplay<TFont, TGlyph> _nucleusDisplay;
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
      Width =
        Math.Max(nucleusDisplay?.Width ?? 0f,
        Math.Max(upperLimit?.Width ?? 0f,
                 lowerLimit?.Width ?? 0f));
      _UpdateComponentPositions();
    }

    // A display representing the numerator of the fraction. Its position is relative
    // to the parent and it is not treated as a sub-display.
    public IDisplay<TFont, TGlyph> UpperLimit { get; private set; }
    // A display representing the numerator of the fraction. Its position is relative
    // to the parent and it is not treated as a sub-display.
    public IDisplay<TFont, TGlyph> LowerLimit { get; private set; }

    public RectangleF DisplayBounds => this.ComputeDisplayBounds();

    public float Ascent =>
      _nucleusDisplay.Ascent +
      (UpperLimit == null ? 0 : _extraPadding + UpperLimit.Ascent + _upperLimitGap + UpperLimit.Descent);

    public float Descent =>
      _nucleusDisplay.Descent +
      (LowerLimit == null ? 0 : _extraPadding + LowerLimit.Ascent + _lowerLimitGap + LowerLimit.Descent);

    public float Width { get; set; }

    public Range Range =>
      _nucleusDisplay.Range + UpperLimit?.Range ?? Range.NotFound + LowerLimit?.Range ?? Range.NotFound;

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
    private void _UpdateNucleusPosition() =>
      _nucleusDisplay.Position = new PointF(Position.X + (Width - _nucleusDisplay.Width) / 2, Position.Y);
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

    public override string ToString() => $@"{{{_nucleusDisplay}}}^{{{UpperLimit}}}_{{{LowerLimit}}}";
  }
}
