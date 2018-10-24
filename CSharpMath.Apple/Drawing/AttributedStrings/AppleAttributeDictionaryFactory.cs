using System;
using System.Collections.Generic;
using System.Text;
using TFont = CSharpMath.Apple.AppleMathFont;
using CSharpMath.Display.Text;
using UIKit;
using TGlyph = System.UInt16;
using CoreText;
using CSharpMath.Structures;

namespace CSharpMath.Apple.Drawing {
  [Obsolete("Is any code using this?", true)]
  public class AppleAttributeDictionaryFactory {
    public static UIStringAttributes FromAttributedGlyphRun(AttributedGlyphRun<TFont, TGlyph> glyphRun, Color color) {
      return new UIStringAttributes {
        ForegroundColor = color.ToUiColor(),
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
