using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;

namespace CSharpMath.Display {
  public class FractionDisplay : IDisplay {
    private PointF _currentPosition;
    private Range _indexRange;

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

    public FractionDisplay(MathListDisplay numeratorDisplay, MathListDisplay denominatorDisplay, PointF currentPosition, Range indexRange) {
      Numerator = numeratorDisplay;
      Denominator = denominatorDisplay;
      _currentPosition = currentPosition;
      _indexRange = indexRange;
    }



    public RectangleF DisplayBounds {
      get {
        var numFrame = Numerator.Frame();
        var denomFrame = Denominator.Frame();
        var r = numFrame.Union(denomFrame);
        return r;
      }
    }
    public float Ascent { get; set; }

    public float Descent {get;set;}

    public float Width { get; set; }

    public Range Range { get; set; }

    public PointF Position { get; set; }

    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext context) => throw new NotImplementedException();
  }
}
