using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;

namespace CSharpMath.Display {
  public class AccentDisplay : IDisplay {
    public MathListDisplay Accentee { get; private set; }
    public GlyphDisplay Accent { get; private set; }

    public RectangleF DisplayBounds => ((IDisplay)this.Accentee).DisplayBounds;

    public float Ascent => ((IDisplay)this.Accentee).Ascent;

    public float Descent => ((IDisplay)this.Accentee).Descent;

    public float Width => ((IDisplay)this.Accentee).Width;

    public Range Range => ((IDisplay)this.Accentee).Range;

    public PointF Position => ((IDisplay)this.Accentee).Position;

    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext context) => ((IDisplay)this.Accentee).Draw(context);
  }
}
