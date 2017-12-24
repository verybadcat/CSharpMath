using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CSharpMath.Atoms;

namespace CSharpMath.Display.Text
{
  public static class AttributedGlyphRuns
  {
    public static AttributedGlyphRun<TFont, TGlyph> Create<TFont, TGlyph>(string text, TGlyph[] glyphs, TFont font, MathColor color)
      where TFont : MathFont<TGlyph>
    {
      var kernedGlyphs = glyphs.Select(g => new KernedGlyph<TGlyph>(g)).ToArray();
      return new AttributedGlyphRun<TFont, TGlyph>
      {
        Text = text,
        KernedGlyphs = kernedGlyphs,
        Font = font,
        TextColor = color
      };
    }

    public static AttributedGlyphRun<TFont, TGlyph> Create<TFont, TGlyph>(string text, TGlyph[] glyphs, TFont font)
      where TFont : MathFont<TGlyph>
    => Create(text, glyphs, font, default(MathColor));


  }
}
