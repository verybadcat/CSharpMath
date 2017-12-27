using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.Display.Text;
using System.Linq;
using CSharpMath.FrontEnd;

namespace CSharpMath.Display {
  class GlyphConstructionDisplay<TFont, TGlyph> : IDownshiftableDisplay<TFont, TGlyph> 
    where TFont : MathFont<TGlyph> {

    private TGlyph[] _glyphs;
    private PointF[] _glyphPositions;
    private TFont _mathFont;
    private int _nGlyphs => _glyphs.Length;

    public float ShiftDown { get; set; }
    public RectangleF DisplayBounds => this.ComputeDisplayBounds();

    public float Ascent { get; set; }

    public float Descent { get; set; }

    public float Width {get;set;}

    public Range Range { get; set; }

    public PointF Position { get; set; }

    public void SetPosition(PointF position) {
      Position = position;
    }

    public bool HasScript { get; set; }

    public GlyphConstructionDisplay(IEnumerable<TGlyph> glyphs, IEnumerable<float> offsets, TFont font) {
      _glyphs = glyphs.ToArray();
      _glyphPositions = offsets.Select(x => new PointF(0, x)).ToArray();
      _mathFont = font;
    }

    public void Draw(IGraphicsContext<TFont, TGlyph> context){
      context.SaveState();
      PointF delta = new PointF(Position.X, Position.Y - ShiftDown);
      context.Translate(delta);
      context.SetTextPosition(new PointF());
      context.DrawGlyphsAtPoints(_glyphs, _mathFont, _glyphPositions);
      context.RestoreState();
    }
  }
}
