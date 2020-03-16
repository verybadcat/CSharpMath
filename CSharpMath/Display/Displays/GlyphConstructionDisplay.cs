using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSharpMath.Atom;
using Color = CSharpMath.Structures.Color;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  public class GlyphConstructionDisplay<TFont, TGlyph> : IGlyphDisplay<TFont, TGlyph> where TFont : IFont<TGlyph> {
    private readonly IReadOnlyList<TGlyph> _glyphs;
    private readonly IEnumerable<PointF> _glyphPositions;

    public float ShiftDown { get; set; }


    public float Ascent { get; set; }

    public float Descent { get; set; }

    public float Width { get; set; }

    public Range Range { get; set; }

    public PointF Position { get; set; }

    public void SetPosition(PointF position) => Position = position;

    public bool HasScript { get; set; }

    public GlyphConstructionDisplay(IReadOnlyList<TGlyph> glyphs, IEnumerable<float> offsets, TFont font) {
      _glyphs = glyphs;
      _glyphPositions = offsets.Select(x => new PointF(0, x));
      Font = font;
    }

    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      context.SaveState();
      context.Translate(new PointF(Position.X, Position.Y - ShiftDown));
      context.SetTextPosition(new PointF());
      context.DrawGlyphsAtPoints(_glyphs, Font, _glyphPositions, TextColor);
      context.RestoreState();
    }
    public Color? TextColor { get; set; }

    public TFont Font { get; }

    public void SetTextColorRecursive(Color? textColor) => TextColor ??= textColor;
  }
}
