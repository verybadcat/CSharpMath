using System.Globalization;
using CoreText;
using CSharpMath.Display;
using Foundation;
using TFont = CSharpMath.Apple.AppleMathFont;
using TGlyph = System.UInt16;

namespace CSharpMath.Apple {
  public static partial class Extensions {
    public static NSMutableAttributedString ToNsAttributedString
      (this AttributedGlyphRun<TFont, TGlyph> glyphRun) {
      var text = glyphRun.Text.ToString();
      var unicodeIndexes = StringInfo.ParseCombiningCharacters(text);
      var attributedString = new NSMutableAttributedString(text, new CTStringAttributes {
        ForegroundColorFromContext = true,
        Font = glyphRun.Font.CtFont
      });
      var kernedGlyphs = glyphRun.GlyphInfos;
      for (int i = 0; i < kernedGlyphs.Count; i++) {
        var range = new NSRange(unicodeIndexes[i],
          (i < unicodeIndexes.Length - 1 ? unicodeIndexes[i + 1] : text.Length)
          - unicodeIndexes[i]);
        if (kernedGlyphs[i].KernAfterGlyph is var kern && !(kern is 0))
          attributedString.AddAttribute
            (CTStringAttributeKey.KerningAdjustment, new NSNumber(kern), range);
        if (kernedGlyphs[i].Foreground is System.Drawing.Color foreground)
          attributedString.AddAttribute(CTStringAttributeKey.ForegroundColor,
            ObjCRuntime.Runtime.GetNSObject(foreground.ToCGColor().Handle), range);
      }
      return attributedString;
    }
  }
}