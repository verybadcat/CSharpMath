using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;

namespace CSharpMath.Display {
  /// <summary>Corresponds to MTLineDisplay in iosMath.</summary> 
  public class OverOrUnderlineDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : MathFont<TGlyph> {
    /// <summary>A display representing the inner list that is underlined.
    /// Its position is relative to the parent. Is not treated as a
    public MathListDisplay<TFont, TGlyph> Inner { get; private set; }

    public RectangleF DisplayBounds => ((IDisplay<TFont, TGlyph>)this.Inner).DisplayBounds;

    public float Ascent => ((IDisplay<TFont, TGlyph>)this.Inner).Ascent;

    public float Descent => ((IDisplay<TFont, TGlyph>)this.Inner).Descent;

    public float Width => ((IDisplay<TFont, TGlyph>)this.Inner).Width;

    public Range Range => ((IDisplay<TFont, TGlyph>)this.Inner).Range;
    public PointF Position { get; set; }
    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) => ((IDisplay<TFont, TGlyph>)this.Inner).Draw(context);

    
  }
}
