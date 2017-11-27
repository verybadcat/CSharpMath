using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;

namespace CSharpMath.Display {
  /// <summary>Corresponds to MTLineDisplay in iosMath.</summary> 
  public class OverOrUnderlineDisplay : IDisplay {
    /// <summary>A display representing the inner list that is underlined.
    /// Its position is relative to the parent. Is not treated as a
    public MathListDisplay Inner { get; private set; }

    public RectangleF DisplayBounds => ((IDisplay)this.Inner).DisplayBounds;

    public float Ascent => ((IDisplay)this.Inner).Ascent;

    public float Descent => ((IDisplay)this.Inner).Descent;

    public float Width => ((IDisplay)this.Inner).Width;

    public Range Range => ((IDisplay)this.Inner).Range;

    public void Draw(IGraphicsContext context) => ((IDisplay)this.Inner).Draw(context);

    /// sub-display.</summary> 
  }
}
