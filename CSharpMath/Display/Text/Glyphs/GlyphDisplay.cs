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
    where TFont : IFont<TGlyph> {

    public RectangleF DisplayBounds => this.ComputeDisplayBounds();

    public float Ascent { get; set; }

    public float Descent { get; set; }

    public float Width { get; set; }

    public Range Range { get; set; }

    public PointF Position { get; set; }

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
      var glyphs = new RentedArray<TGlyph>(Glyph);
      var positions = new RentedArray<PointF>(new PointF());
      context.Translate(new PointF(Position.X, Position.Y - ShiftDown));
      context.SetTextPosition(new PointF());
      context.DrawGlyphsAtPoints(glyphs.Result, Font, positions.Result, TextColor);
      context.RestoreState();
      glyphs.Return();
      positions.Return();
    }
    public Color? TextColor { get; set; }

    public void SetTextColorRecursive(Color? textColor) {
      TextColor = TextColor ?? textColor;
    }
  }
}
