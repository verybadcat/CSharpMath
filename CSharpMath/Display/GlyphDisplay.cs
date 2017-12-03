using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.Display.Text;

namespace CSharpMath.Display {
  public class GlyphDisplay<TGlyph> : IDownshiftableDisplay {
   
    public RectangleF DisplayBounds => this.ComputeDisplayBounds();

    public float Ascent  {get;set;}

    public float Descent { get; set; }

    public float Width  {get;set;}

    public Range Range  {get;set;}

    public PointF Position { get; set; }
    public void SetPosition(PointF value) =>
      Position = value;

    public bool HasScript { get; set; }
    public float ShiftDown { get; set; }

    public TGlyph Glyph { get; set; } 

    public MathFont Font { get; set; }
    public GlyphDisplay(TGlyph glyph, Range range, MathFont font) {
      Glyph = glyph;
      Range = range;
      Font = font;
    }
    public void Draw(IGraphicsContext context) => throw new NotImplementedException();
  }
}
