using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;

namespace CSharpMath.Display.Text {
  public static class AttributedGlyphRuns {
    public static AttributedGlyphRun<TMathFont, TGlyph> Create<TMathFont, TGlyph>(string text, TGlyph[] glyphs, TMathFont font, MathColor color)
      where TMathFont: MathFont<TGlyph>
      => new AttributedGlyphRun<TMathFont, TGlyph> {
        Text = text,
        Glyphs = glyphs,
        Font = font,
        TextColor = color
      };

    public static AttributedGlyphRun<TMathFont, TGlyph> Create<TMathFont, TGlyph>(string text, TGlyph[] glyphs, TMathFont font)
      where TMathFont : MathFont<TGlyph>
      => new AttributedGlyphRun<TMathFont, TGlyph> {
      Text = text,
      Glyphs = glyphs,
        Font = font
      };

    public static AttributedGlyphRun<TMathFont, TGlyph> Create<TMathFont, TGlyph>(TGlyph[] text, MathColor color)
      where TMathFont: MathFont<TGlyph>
      => new AttributedGlyphRun<TMathFont, TGlyph> {
        Glyphs = text,
        TextColor = color
      };
  }
}
