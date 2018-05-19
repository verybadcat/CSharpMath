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

    public Glyph FindGlyphForCharacterAtIndex(int index, string str) =>
      _typeface.Lookup(char.ConvertToUtf32(str, index - (char.IsLowSurrogate(str[index]) ? 1 : 0)));

    public Glyph[] FindGlyphs(string str) => StringUtils.GetCodepoints(str.ToCharArray()).Select(_typeface.Lookup).ToArray();

    public bool GlyphIsEmpty(Glyph glyph) => glyph == null || glyph == Glyph.Empty;
  }
}
