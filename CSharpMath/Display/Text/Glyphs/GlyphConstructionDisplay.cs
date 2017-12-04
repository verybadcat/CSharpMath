using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.Display.Text;
using System.Linq;

namespace CSharpMath.Display {
  class GlyphConstructionDisplay<TGlyph> : IDownshiftableDisplay {

    private TGlyph[] _glyphs;
    private PointF[] _glyphPositions;
    private MathFont<TGlyph> _mathFont;
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

    public GlyphConstructionDisplay(IEnumerable<TGlyph> glyphs, IEnumerable<float> offsets, MathFont<TGlyph> font) {
      _glyphs = glyphs.ToArray();
      _glyphPositions = offsets.Select(x => new PointF(0, x)).ToArray();
      _mathFont = font;
    }

    public void Draw(IGraphicsContext context) => throw new NotImplementedException();
  }
}
