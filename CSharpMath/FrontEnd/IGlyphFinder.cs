using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.FrontEnd {
  public interface IGlyphFinder<TGlyph> {
    TGlyph FindGlyphForCharacterAtIndex(int index, string str);
    TGlyph[] FindGlyphs(string str);
    bool GlyphIsEmpty(TGlyph glyph);
  }
}