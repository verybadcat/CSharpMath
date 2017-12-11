using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;

namespace CSharpMath.Display {
  public class AccentDisplay<TGlyph> : IDisplay<TGlyph> {
    public MathListDisplay<TGlyph> Accentee { get; private set; }
    public GlyphDisplay<TGlyph> Accent { get; private set; }

    public RectangleF DisplayBounds => ((IDisplay<TGlyph>)this.Accentee).DisplayBounds;

    public float Ascent => ((IDisplay<TGlyph>)this.Accentee).Ascent;

    public float Descent => ((IDisplay<TGlyph>)this.Accentee).Descent;

    public float Width => ((IDisplay<TGlyph>)this.Accentee).Width;

    public Range Range => ((IDisplay<TGlyph>)this.Accentee).Range;

    public PointF Position => ((IDisplay<TGlyph>)this.Accentee).Position;

    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext<TGlyph> context) => ((IDisplay<TGlyph>)this.Accentee).Draw(context);
  }
}
