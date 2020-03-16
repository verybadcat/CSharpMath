using CSharpMath.Display.FrontEnd;
using TFont = CSharpMath.Apple.AppleMathFont;
using TGlyph = System.UInt16;
using System.Globalization;
using System.Collections.Generic;

namespace CSharpMath.Apple {
  public class CtFontGlyphFinder : IGlyphFinder<TFont, TGlyph> {
    private CtFontGlyphFinder() { }
    public static CtFontGlyphFinder Instance { get; } = new CtFontGlyphFinder();
    public IEnumerable<ushort> FindGlyphs(TFont font, string str) {
      // not completely sure this is correct. Need an actual
      // example of a composed character sequence coming from LaTeX.
      var unicodeIndexes = StringInfo.ParseCombiningCharacters(str);
      foreach (var index in unicodeIndexes) {
        yield return FindGlyphForCharacterAtIndex(font, index, str);
      }
    }
    public bool GlyphIsEmpty(TGlyph glyph) => glyph == 0;
    public TGlyph EmptyGlyph => 0;
    public TGlyph FindGlyphForCharacterAtIndex(TFont font, int index, string str) {
      var unicodeIndices = StringInfo.ParseCombiningCharacters(str);
      int start = 0;
      int end = str.Length;
      foreach (var unicodeIndex in unicodeIndices) {
        if (unicodeIndex <= index) {
          start = unicodeIndex;
        } else {
          end = unicodeIndex;
          break;
        }
      }
      int length = end - start;
      TGlyph[] glyphs = new TGlyph[length];
      font.CtFont.GetGlyphsForCharacters(
        str.Substring(start, length).ToCharArray(), glyphs, length);
      return glyphs[0];
    }
  }
}
