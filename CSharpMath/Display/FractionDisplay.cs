using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;

namespace CSharpMath.Display {
  public class FractionDisplay : IDisplay {
     // A display representing the numerator of the fraction. Its position is relative
     //to the parent and it is not treated as a sub-display.
 
    public MathListDisplay Numerator { get; private set; }
    // A display representing the numerator of the fraction. Its position is relative
    //to the parent and it is not treated as a sub-display.
    public MathListDisplay Denominator { get; private set; }

    public RectangleF DisplayBounds {
      get {
        var numFrame = Numerator.Frame();
        var denomFrame = Denominator.Frame();
        var r = numFrame.Union(denomFrame);
        return r;
      }
    }
    public float Ascent => throw new NotImplementedException();

    public float Descent => throw new NotImplementedException();

    public float Width => throw new NotImplementedException();

    public Range Range => throw new NotImplementedException();

    public PointF Position => throw new NotImplementedException();

    public void Draw(IGraphicsContext context) => throw new NotImplementedException();
  }
}
