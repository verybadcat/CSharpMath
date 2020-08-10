using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  public class GlyphConstructionDisplay<TFont, TGlyph> : IGlyphDisplay<TFont, TGlyph> where TFont : IFont<TGlyph> {
    private readonly IReadOnlyList<TGlyph> _glyphs;
    private readonly IEnumerable<PointF> _glyphPositions;

    public float ShiftDown { get; set; }

    readonly float _ascent;
    readonly float _descent;
    public float Ascent => _ascent - ShiftDown;
    public float Descent => _descent + ShiftDown;

    public float Width { get; }

    public Range Range { get; set; }

    public PointF Position { get; set; }

    public void SetPosition(PointF position) => Position = position;

    public bool HasScript { get; set; }

    public GlyphConstructionDisplay(
      IReadOnlyList<TGlyph> glyphs, IEnumerable<float> offsets, TFont font,
      float ascent, float descent, float width) {
      _glyphs = glyphs;
      _glyphPositions = offsets.Select(x => new PointF(0, x));
      Font = font;
      _ascent = ascent;
      _descent = descent;
      Width = width;
    }

    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      context.SaveState();
      context.Translate(new PointF(Position.X, Position.Y - ShiftDown));
      context.SetTextPosition(new PointF());
      context.DrawGlyphsAtPoints(_glyphs, Font, _glyphPositions, TextColor);
      context.RestoreState();
    }

    public TFont Font { get; }

    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) => TextColor ??= textColor;
    public Color? BackColor { get; set; }
  }
}
