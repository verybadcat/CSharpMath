using CSharpMath.FrontEnd;
using TGlyph = System.UInt16;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System;
using CoreText;

namespace CSharpMath.Apple {
  public class CtFontGlyphFinder : IGlyphFinder<TGlyph> {
    private readonly CTFont _ctFont;

    public CtFontGlyphFinder(CTFont ctFontPointSizeIrrelevant) {
      _ctFont = ctFontPointSizeIrrelevant;
    }
    private IEnumerable<TGlyph> ToUintEnumerable(byte[] bytes) {
      for (int i = 0; i < bytes.Length; i += 2) {
        if (i == bytes.Length - 1) {
          yield return bytes[i];
        } else {
          yield return BitConverter.ToUInt16(bytes, i);
        }
      }
    }

    public TGlyph[] ToUintArray(byte[] bytes) {
      return ToUintEnumerable(bytes).ToArray();
    }


    public byte[] ToByteArray(TGlyph[] glyphs) {
      byte[] r = new byte[glyphs.Length * 2];
      for (int i = 0; i < glyphs.Length; i++) {
        byte[] localBytes = BitConverter.GetBytes(glyphs[i]);
        r[2 * i] = localBytes[0];
        r[1 + 2 * i] = localBytes[1];
      }
      return r;
    }

    private IEnumerable<ushort> FindGlyphsInternal(string str) {
      // not completely sure this is correct. Need an actual
      // example of a composed character sequence coming from LaTeX.
      var unicodeIndexes = StringInfo.ParseCombiningCharacters(str);
      foreach (var index in unicodeIndexes) {
        yield return FindGlyphForCharacterAtIndex(index, str);
      }
    }

    public bool GlyphIsEmpty(TGlyph glyph)
      => glyph == 0;

    public TGlyph FindGlyphForCharacterAtIndex(int index, string str) {

      var unicodeIndexes = StringInfo.ParseCombiningCharacters(str);
      int start = 0;
      int end = str.Length;
      foreach (var unicodeIndex in unicodeIndexes) {
        if (unicodeIndex <= index) {
          start = unicodeIndex;
        } else {
          end = unicodeIndex;
          break;
        }
      }
      int length = end - start;
      TGlyph[] glyphs = new TGlyph[length];
      char[] chars = str.Substring(start, length).ToCharArray();
      _ctFont.GetGlyphsForCharacters(chars, glyphs, length);
      return glyphs[0];
    }

    public TGlyph[] FindGlyphs(string str)
      => FindGlyphsInternal(str).ToArray();

  }
}
