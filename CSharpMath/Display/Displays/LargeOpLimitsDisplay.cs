using System;
using System.Drawing;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  public class LargeOpLimitsDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    private readonly float _limitShift;
    private readonly int _extraPadding;
    private readonly float _upperLimitGap;
    private readonly float _lowerLimitGap;

    public LargeOpLimitsDisplay(IDisplay<TFont, TGlyph> nucleusDisplay,
      ListDisplay<TFont, TGlyph>? upperLimit,
      float upperLimitGap,
      ListDisplay<TFont, TGlyph>? lowerLimit,
      float lowerLimitGap,
      float limitShift,
      int extraPadding) {
      NucleusDisplay = nucleusDisplay;
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
    public IDisplay<TFont, TGlyph> NucleusDisplay { get; }
    ///<summary>A display representing the upper limit of the large operator.
    ///Its position is relative to the parent and it is not treated as a sub-display.</summary>
    public ListDisplay<TFont, TGlyph>? UpperLimit { get; }
    ///<summary>A display representing the lower limit of the large operator.
    ///Its position is relative to the parent and it is not treated as a sub-display.</summary>
    public ListDisplay<TFont, TGlyph>? LowerLimit { get; }
    
    public float Ascent =>
      NucleusDisplay.Ascent +
      (_extraPadding + UpperLimit?.Ascent + _upperLimitGap + UpperLimit?.Descent ?? 0);

    public float Descent =>
      NucleusDisplay.Descent +
      (_extraPadding + LowerLimit?.Ascent + _lowerLimitGap + LowerLimit?.Descent ?? 0);

    public float Width { get; }

    public Range Range => NucleusDisplay.Range;

    PointF _position;
    public PointF Position { get => _position; set { _position = value; UpdateComponentPositions(); } }

    public bool HasScript { get; set; }

    private void UpdateComponentPositions() {
      NucleusDisplay.Position = new PointF(Position.X + (Width - NucleusDisplay.Width) / 2, Position.Y);
      if (UpperLimit!=null) {
        UpperLimit.Position = new PointF(
          Position.X + _limitShift + (Width - UpperLimit.Width) / 2,
          Position.Y + NucleusDisplay.Ascent + _upperLimitGap + UpperLimit.Descent);
      }
      if (LowerLimit!=null) {
        LowerLimit.Position = new PointF(
          Position.X - _limitShift + (Width - LowerLimit.Width) / 2,
          Position.Y - NucleusDisplay.Descent - _lowerLimitGap - LowerLimit.Ascent);
      }
    }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      UpperLimit?.Draw(context);
      LowerLimit?.Draw(context);
      NucleusDisplay.Draw(context);
    }

    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) {
      TextColor ??= textColor;
      UpperLimit?.SetTextColorRecursive(textColor);
      LowerLimit?.SetTextColorRecursive(textColor);
    }
    public Color? BackColor { get; set; }

    public override string ToString() => $@"{{{NucleusDisplay}}}^{{{UpperLimit}}}_{{{LowerLimit}}}";
  }
}
