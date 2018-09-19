using System;
using System.Globalization;
using CoreText;
using CSharpMath.Display.Text;
using Foundation;
using TFont = CSharpMath.Apple.AppleMathFont;
using TGlyph = System.UInt16;

namespace CSharpMath.Apple.Drawing
{
  public static class AppleAttributedStringFactory
  {
    public static NSMutableAttributedString ToNsAttributedString(this AttributedGlyphRun<TFont, TGlyph> glyphRun) {
      var font = glyphRun.Font;
      var text = glyphRun.Text.ToString();
      var unicodeIndexes = StringInfo.ParseCombiningCharacters(text);
      var attributes = new CTStringAttributes
      {
        ForegroundColorFromContext = true,
        Font = font.CtFont
      };
      var attributedString = new NSMutableAttributedString(text, attributes);
      var kernedGlyphs = glyphRun.KernedGlyphs;
      for (int i = 0; i < kernedGlyphs.Count; i++) {
        var kern = kernedGlyphs[i].KernAfterGlyph;
        if (kern!=0) {
          var endIndex = (i < unicodeIndexes.Length - 1) ? unicodeIndexes[i + 1] : text.Length;
          var range = new NSRange(unicodeIndexes[i], endIndex - unicodeIndexes[i]);
          attributedString.AddAttribute(CTStringAttributeKey.KerningAdjustment, new NSNumber(kern), range);
        }
      }
      return attributedString;
    }
  }
}
