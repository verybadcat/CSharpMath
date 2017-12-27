using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.FrontEnd {
  /// <remarks>For changing a string into glyphs which will appear on the page. </remarks>
  public interface IGlyphFinder<TGlyph> {
    TGlyph FindGlyphForCharacterAtIndex(int index, string str);
    TGlyph[] FindGlyphs(string str);
    bool GlyphIsEmpty(TGlyph glyph);
  }
}