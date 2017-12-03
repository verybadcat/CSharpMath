using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Display.Text {
  public static class AttributedGlyphRuns {
    public static AttributedGlyphRun<TGlyph> Create<TGlyph>(TGlyph[] text, MathFont font, Color color)
      => new AttributedGlyphRun<TGlyph> {
        Text = text,
        Font = font,
        TextColor = color
      };

    public static AttributedGlyphRun<TGlyph> Create<TGlyph>(TGlyph[] text, MathFont font)
      => new AttributedGlyphRun<TGlyph> {
        Text = text,
        Font = font
      };

    public static AttributedGlyphRun<TGlyph> Create<TGlyph>(TGlyph[] text, Color color)
      => new AttributedGlyphRun<TGlyph> {
        Text = text,
        TextColor = color
      };
  }
}
