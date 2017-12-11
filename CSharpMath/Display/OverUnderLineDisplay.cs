using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;

namespace CSharpMath.Display {
  /// <summary>Corresponds to MTLineDisplay in iosMath.</summary> 
  public class OverOrUnderlineDisplay<TGlyph> : IDisplay<TGlyph> {
    /// <summary>A display representing the inner list that is underlined.
    /// Its position is relative to the parent. Is not treated as a
    public MathListDisplay<TGlyph> Inner { get; private set; }

    public RectangleF DisplayBounds => ((IDisplay<TGlyph>)this.Inner).DisplayBounds;

    public float Ascent => ((IDisplay<TGlyph>)this.Inner).Ascent;

    public float Descent => ((IDisplay<TGlyph>)this.Inner).Descent;

    public float Width => ((IDisplay<TGlyph>)this.Inner).Width;

    public Range Range => ((IDisplay<TGlyph>)this.Inner).Range;
    public PointF Position { get; set; }
    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext<TGlyph> context) => ((IDisplay<TGlyph>)this.Inner).Draw(context);

    
  }
}
