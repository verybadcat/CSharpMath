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
    public static AttributedGlyphRun<TFont, TGlyph> Create<TFont, TGlyph>(string text, IEnumerable<TGlyph> glyphs, TFont font, bool isPlaceHolder)
      where TFont : IFont<TGlyph>
    {
      var kernedGlyphs = glyphs.Select(g => new KernedGlyph<TGlyph>(g)).ToList();
      return new AttributedGlyphRun<TFont, TGlyph>
      {
        Placeholder = isPlaceHolder,
        Text = new StringBuilder(text),
        KernedGlyphs = kernedGlyphs,
        Font = font
      };
    }
  }
}
