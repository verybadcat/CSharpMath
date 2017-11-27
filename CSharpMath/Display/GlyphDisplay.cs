using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;

namespace CSharpMath.Display {
  public class GlyphDisplay : IDisplay {
    public RectangleF DisplayBounds => throw new NotImplementedException();

    public float Ascent => throw new NotImplementedException();

    public float Descent => throw new NotImplementedException();

    public float Width => throw new NotImplementedException();

    public Range Range => throw new NotImplementedException();

    public PointF Position => throw new NotImplementedException();

    public void Draw(IGraphicsContext context) => throw new NotImplementedException();
  }
}
