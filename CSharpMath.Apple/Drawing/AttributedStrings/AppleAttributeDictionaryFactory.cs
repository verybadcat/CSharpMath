using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Display.Text;
using UIKit;
using TGlyph = System.UInt16;

namespace CSharpMath.Apple.Drawing {
  public class AppleAttributeDictionaryFactory {
    public static UIStringAttributes FromAttributedGlyphRun(AttributedGlyphRun<AppleMathFont, TGlyph> glyphRun) {
      return new UIStringAttributes {
        ForegroundColor = glyphRun.TextColor.ToNative(),
        Font = UIFont.SystemFontOfSize(glyphRun.Font.PointSize)
      };
    }
  }
}
