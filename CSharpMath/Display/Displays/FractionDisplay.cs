using System;
using System.Drawing;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  public class FractionDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    private PointF _position;
    ///<summary>A display representing the numerator of the fraction.
    ///Its position is relative to the parent and it is not treated as a sub-display.</summary>
    public ListDisplay<TFont, TGlyph> Numerator { get; }
    ///<summary>A display representing the denominator of the fraction.
    ///Its position is relative to the parent and it is not treated as a sub-display.</summary>
    public ListDisplay<TFont, TGlyph> Denominator { get; }

    public float NumeratorUp { get; set; }
    public float DenominatorDown { get; set; }
    public float LineThickness { get; set; }
    public float LinePosition { get; set; }

    public Range Range { get; }

    public FractionDisplay(ListDisplay<TFont, TGlyph> numeratorDisplay, ListDisplay<TFont, TGlyph> denominatorDisplay, PointF currentPosition, Range range) {
      Numerator = numeratorDisplay;
      Denominator = denominatorDisplay;
      _position = currentPosition;
      Range = range;
      UpdateNumeratorAndDenominatorPositions();
    }
    
    public float Ascent => Numerator.Ascent + NumeratorUp;

    public float Descent => Denominator.Descent + DenominatorDown;

    public float Width => Math.Max(Numerator.Width, Denominator.Width);

    public void UpdateNumeratorAndDenominatorPositions() {
      Numerator.Position =
        new PointF(Position.X + (Width - Numerator.Width) / 2, Position.Y + NumeratorUp);
      Denominator.Position =
        new PointF(Position.X + (Width - Denominator.Width) / 2, Position.Y - DenominatorDown);
    }
    public PointF Position {
      get => _position;
      set {
        _position = value;
        UpdateNumeratorAndDenominatorPositions();
      }
    }

    public bool HasScript { get; set; }

    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      Numerator.Draw(context);
      Denominator.Draw(context);
      context.SaveState();
      context.DrawLine(Position.X, Position.Y + LinePosition, Position.X + Width, Position.Y + LinePosition, LineThickness, TextColor);
      context.RestoreState();
    }
    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) {
      TextColor ??= textColor;
      Numerator.SetTextColorRecursive(textColor);
      Denominator.SetTextColorRecursive(textColor);
    }
    public Color? BackColor { get; set; }

    public override string ToString() => $@"\frac{{{Numerator}}}{{{Denominator}}}";
  }
}
