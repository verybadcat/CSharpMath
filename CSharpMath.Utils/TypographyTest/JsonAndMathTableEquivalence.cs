using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CSharpMath.DevUtils.TypographyTest {
  static class JsonAndMathTableEquivalence {
    public static bool Test() {
      var Return = true;
      var json = CSharpMath.SkiaSharp.SkiaResources.Json;
      var typeface = _Statics.GlyphLayout.Typeface;
      #region MathConsts
       foreach (var prop in ((JObject)json).Property("constants").Children<JProperty>()) {
         Return &= prop.Value == ((dynamic)typeface.MathConsts)[prop.Name];
       }
      #endregion
      #region Italic
      foreach (var glyph in typeface.Glyphs) {
        short? lhs, rhs;
        try {
          lhs = glyph.MathGlyphInfo.ItalicCorrection.Value.Value;
        } catch {
          lhs = null;
        }
        try {
          rhs = json["italic"][glyph.GetCff1GlyphData().Name].Value<short>();
        } catch {
          rhs = null;
        }
        Return &= lhs == rhs;
      }
      #endregion
      return true;
    }
  }
}
