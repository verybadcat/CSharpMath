using CSharpMath.FrontEnd;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System;
using Typography.OpenFont;

namespace CSharpMath.SkiaSharp {
  public class GlyphFinder : IGlyphFinder<Glyph> {
    private readonly Typeface _typeface;

    public GlyphFinder(Typeface typeface) => _typeface = typeface;

    public Glyph FindGlyphForCharacterAtIndex(int index, string str) {
      throw new NotImplementedException();
    }

    public Glyph[] FindGlyphs(string str) {
      var unicodeIndices = StringInfo.ParseCombiningCharacters(str);
      for (int i = 0; i < unicodeIndices.Length; i++) {
        var current = unicodeIndices[i];

        //0 = next does not exist; any other value = index of next
        var next = unicodeIndices.ElementAtOrDefault(i + 1);
        
        _typeface.Lookup()
      }
      Glyph[] glyphs = new Glyph[length];
      char[] chars = str.Substring(start, length).ToCharArray();
    }

    public bool GlyphIsEmpty(Glyph glyph) => glyph.HasGlyphInstructions;
  }
}
