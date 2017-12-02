using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;

namespace CSharpMath.Display {
  public class FractionDisplay : IDisplay {
    private PointF _currentPosition;
    private Range _range;

    // A display representing the numerator of the fraction. Its position is relative
    //to the parent and it is not treated as a sub-display.

    public MathListDisplay Numerator { get; private set; }
    // A display representing the numerator of the fraction. Its position is relative
    //to the parent and it is not treated as a sub-display.
    public MathListDisplay Denominator { get; private set; }

    public float NumeratorUp { get; set; }
    public float DenominatorDown { get; set; }
    public float LineThickness { get; set; }
    public float LinePosition { get; set; }

    public Range Range => _range;

    public FractionDisplay(MathListDisplay numeratorDisplay, MathListDisplay denominatorDisplay, PointF currentPosition, Range range) {
      Numerator = numeratorDisplay;
      Denominator = denominatorDisplay;
      _currentPosition = currentPosition;
      _range = range;
    }



    public RectangleF DisplayBounds {
      get {
        var numFrame = Numerator.Frame();
        var denomFrame = Denominator.Frame();
        var r = numFrame.Union(denomFrame);
        return r;
      }
    }
    public float Ascent => Numerator.Ascent + NumeratorUp;

    public float Descent => Denominator.Descent + DenominatorDown;

    public float Width => Math.Max(Numerator.Width, Denominator.Width);

    private void _UpdateNumeratorPosition() => Numerator.Position = new PointF(
        this.Position.X + (this.Width - Numerator.Width) / 2,
        this.Position.Y + this.NumeratorUp);

    private void _UpdateDenominatorPosition()
      => Denominator.Position = new PointF(
        this.Position.X + (this.Width - Denominator.Width) / 2,
        this.Position.Y - this.DenominatorDown);

    private PointF _Position;
    public PointF Position {
      get => _Position;
      set {
        _Position = value;
        _UpdateNumeratorPosition();
        _UpdateDenominatorPosition();
      }
    }

    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext context) => throw new NotImplementedException();
  }
}
