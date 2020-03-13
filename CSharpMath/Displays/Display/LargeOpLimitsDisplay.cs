using System;
using System.Drawing;
using CSharpMath.Atoms;
using Color = CSharpMath.Structures.Color;

namespace CSharpMath.Displays.Display {
  using FrontEnd;
  public class LargeOpLimitsDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    private readonly IDisplay<TFont, TGlyph> _nucleusDisplay;
    private readonly float _limitShift;
    private readonly int _extraPadding;
    private readonly float _upperLimitGap;
    private readonly float _lowerLimitGap;

    public LargeOpLimitsDisplay(IDisplay<TFont, TGlyph> nucleusDisplay,
      IDisplay<TFont, TGlyph>? upperLimit,
      float upperLimitGap,
      IDisplay<TFont, TGlyph>? lowerLimit,
      float lowerLimitGap,
      float limitShift,
      int extraPadding) {
      _nucleusDisplay = nucleusDisplay;
      UpperLimit = upperLimit;
      _upperLimitGap = upperLimitGap;
      LowerLimit = lowerLimit;
      _lowerLimitGap = lowerLimitGap;
      _limitShift = limitShift;
      _extraPadding = extraPadding; // corresponds to \xi_13 in TeX.
      Width =
        Math.Max(nucleusDisplay?.Width ?? 0f,
          Math.Max(upperLimit?.Width ?? 0f, lowerLimit?.Width ?? 0f));
      UpdateComponentPositions();
    }

    ///<summary>A display representing the numerator of the fraction.
    ///Its position is relative to the parent and it is not treated as a sub-display.</summary>
    public IDisplay<TFont, TGlyph>? UpperLimit { get; private set; }
    ///<summary>A display representing the numerator of the fraction.
    ///Its position is relative to the parent and it is not treated as a sub-display.</summary>
    public IDisplay<TFont, TGlyph>? LowerLimit { get; private set; }
    public RectangleF DisplayBounds => this.DisplayBounds();
    public float Ascent =>
      _nucleusDisplay.Ascent +
      (_extraPadding + UpperLimit?.Ascent + _upperLimitGap + UpperLimit?.Descent ?? 0);

    public float Descent =>
      _nucleusDisplay.Descent +
      (_extraPadding + LowerLimit?.Ascent + _lowerLimitGap + LowerLimit?.Descent ?? 0);

    public float Width { get; set; }

    public Range Range =>
      _nucleusDisplay.Range + (UpperLimit?.Range ?? Range.NotFound) + (LowerLimit?.Range ?? Range.NotFound);

    PointF _position;
    public PointF Position { get => _position; set { _position = value; UpdateComponentPositions(); } }

    public bool HasScript { get; set; }

    private void UpdateComponentPositions() {
      _nucleusDisplay.Position = new PointF(Position.X + (Width - _nucleusDisplay.Width) / 2, Position.Y);
      if (UpperLimit!=null) {
        UpperLimit.Position = new PointF(
          Position.X + _limitShift + (Width - UpperLimit.Width) / 2,
          Position.Y + _nucleusDisplay.Ascent + _upperLimitGap + UpperLimit.Descent);
      }
      if (LowerLimit!=null) {
        LowerLimit.Position = new PointF(
          Position.X - _limitShift + (Width - LowerLimit.Width) / 2,
          Position.Y - _nucleusDisplay.Descent - _lowerLimitGap - LowerLimit.Ascent);
      }
    }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      UpperLimit?.Draw(context);
      LowerLimit?.Draw(context);
      _nucleusDisplay.Draw(context);
    }

    public Color? TextColor { get; set; }

    public void SetTextColorRecursive(Color? textColor) {
      TextColor ??= textColor;
      UpperLimit?.SetTextColorRecursive(textColor);
      LowerLimit?.SetTextColorRecursive(textColor);
    }

    public override string ToString() => $@"{{{_nucleusDisplay}}}^{{{UpperLimit}}}_{{{LowerLimit}}}";
  }
}
