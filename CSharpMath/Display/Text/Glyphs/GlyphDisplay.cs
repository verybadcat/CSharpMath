using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using Color = CSharpMath.Structures.Color;

namespace CSharpMath.Display {
  public class GlyphDisplay<TFont, TGlyph> : IDownshiftableDisplay<TFont, TGlyph> 
    where TFont : MathFont<TGlyph> {
   
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

    public TFont Font { get; set; }
    public GlyphDisplay(TGlyph glyph, Range range, TFont font) {
      Glyph = glyph;
      Range = range;
      Font = font;
    }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      context.SaveState();
      TGlyph[] glyphArray = { Glyph };
      PointF[] positions = { new PointF() };
      context.Translate(new PointF(Position.X, Position.Y - ShiftDown));
      context.SetTextPosition(new PointF());
      context.DrawGlyphsAtPoints(glyphArray, Font, positions, TextColor);
      context.RestoreState();
    }
    public Color? TextColor { get; set; }

    public void SetTextColor(Color? textColor) {
      TextColor = TextColor ?? textColor;
    }
  }
}
