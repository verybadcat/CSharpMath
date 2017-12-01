using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.Display.Text;
using GlyphType = System.Char; // type of this will likely change at some point

namespace CSharpMath.Display {
  public class GlyphDisplay : IDownshiftableDisplay {
   
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

    public GlyphType Glyph { get; set; } 

    public MathFont Font { get; set; }
    public GlyphDisplay(GlyphType glyph, Range range, MathFont font) {
      Glyph = glyph;
      Range = range;
      Font = font;
    }
    public void Draw(IGraphicsContext context) => throw new NotImplementedException();
  }
}
