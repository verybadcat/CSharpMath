using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TGlyph = System.UInt16;
using TFont = CSharpMath.Apple.AppleMathFont;
using CSharpMath.FrontEnd;
using System.Diagnostics;
using UIKit;
using CoreText;
using Foundation;

namespace CSharpMath.Apple.Drawing {
  public class AppleGraphicsContext : IGraphicsContext<TFont, TGlyph> {

    public AppleGraphicsContext() {
      GlyphFinder = new UnicodeGlyphFinder();
    }
    public CGContext CgContext { get; set; }

    public IGlyphFinder<TGlyph> GlyphFinder { get; set; }

    public void DrawGlyphsAtPoint(ushort[] glyphs, TFont font, PointF point, float maxWidth = float.NaN) {
      var text = GlyphFinder.FindStringDebugPurposesOnly(glyphs);
      DrawTextAtPoint(text, font, point, maxWidth);
    }

    public void DrawLine(float x1, float y1, float x2, float y2) {
      CgContext.MoveTo(x1, y1);
      CgContext.AddLineToPoint(x2, y2);
      CgContext.DrawPath(CGPathDrawingMode.Stroke);
    }

    public void DrawTextAtPoint(string text, TFont font, PointF point, float maxWidth = float.NaN) {
      text.LogCharacters();
      var attributes = new CTStringAttributes
      {
        ForegroundColorFromContext = true,
        Font = font.CtFont
      };
      CgContext.SetStrokeColor(UIColor.Red.CGColor);
      CgContext.TextPosition = point;
      var attributedString = new NSAttributedString(text, attributes);
      using (var textLine = new CTLine(attributedString)) {
        textLine.Draw(CgContext);
      }
    }

    public void RestoreState() {
      CgContext.RestoreState();
    }

    public void SaveState() {
      CgContext.SaveState();
    }

    public void SetTextPosition(PointF position) {
      CgContext.TextPosition = position;
    }

    public void Translate(PointF dxy) {
      CgContext.TranslateCTM(dxy.X, dxy.Y);
    }


  }
}
