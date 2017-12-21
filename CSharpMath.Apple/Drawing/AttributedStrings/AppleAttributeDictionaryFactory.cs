using System;
using System.Collections.Generic;
using System.Text;
using TFont = CSharpMath.Apple.AppleMathFont;
using CSharpMath.Display.Text;
using UIKit;
using TGlyph = System.UInt16;
using CoreText;

namespace CSharpMath.Apple.Drawing {
  public class AppleAttributeDictionaryFactory {
    public static UIStringAttributes FromAttributedGlyphRun(AttributedGlyphRun<TFont, TGlyph> glyphRun) {
      return new UIStringAttributes {
        ForegroundColor = glyphRun.TextColor.ToNative(),
        Font = UIFont.SystemFontOfSize(glyphRun.Font.PointSize)
      };
    }

    public static CTStringAttributes CtFromAttributedGlyphRun(AttributedGlyphRun<TFont, TGlyph> glyphRun) {
      return new CTStringAttributes()
      {
        ForegroundColorFromContext = true,
        Font = glyphRun.Font.CtFont
      };
    }
  }
}
