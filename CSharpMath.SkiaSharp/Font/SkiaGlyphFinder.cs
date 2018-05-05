using CSharpMath.FrontEnd;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;
using Typography.OpenFont;

namespace CSharpMath.SkiaSharp {
  public class SkiaGlyphFinder : IGlyphFinder<Glyph> {
    private readonly Typeface _typeface;

    public SkiaGlyphFinder(Typeface typeface) => _typeface = typeface;

    public static List<int> StringToCodepoints(string str) {
      //get array with correct number of codepoints
      var codepoints = new List<int>();
      //enumerate the string
      char current, next;
      for (int strIndex = 0, cpIndex = 0; strIndex < str.Length; strIndex++, cpIndex++) {
        current = str[strIndex];
        //check for surrogate pairs (reference Unicode 3.0 section "3.7 Surrogates").
        if (current >= 0xD800 && current <= 0xDBFF/* high surrogate */) {
          next = str[strIndex + 1];
          if (next >= 0xDC00 && next <= 0xDFFF/* low surrogate */) {
            //combine the surrogate pair and store it escaped.
            codepoints.Add((current - 0xD800) * 0x400 + next - 0xDC00 + 0x10000);
            //advance index one extra since we already used that char here.
            strIndex++;
            continue;
          }
        }
        codepoints.Add(current);
      }
      return codepoints;
    }


    public Glyph FindGlyphForCharacterAtIndex(int index, string str) => _typeface.Lookup(StringToCodepoints(str)[index]);

    public Glyph[] FindGlyphs(string str) {
      var codepoints = StringToCodepoints(str);
      var glyphs = new Glyph[codepoints.Count];
      for (int i = 0; i < codepoints.Count; i++) {
        glyphs[i] = _typeface.Lookup(codepoints[i]);
      }
      return glyphs;
    }

    public bool GlyphIsEmpty(Glyph glyph) => glyph is null;
  }
}
