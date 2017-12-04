using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath {
  public interface IGlyphNameProvider<TGlyph> {
    string GetGlyphName(TGlyph glyph);
    TGlyph GetGlyph(string glyphName);
  }
}
