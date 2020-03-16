using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CoreGraphics;
using CoreText;
using CSharpMath.Display;
using Color = CSharpMath.Structures.Color;
using TFont = CSharpMath.Apple.AppleMathFont;
using TGlyph = System.UInt16;

namespace CSharpMath.Apple {
  public class AppleGraphicsContext : Display.FrontEnd.IGraphicsContext<TFont, TGlyph> {

    public CGContext CgContext { get; set; }

    public void DrawGlyphsAtPoints
      (IReadOnlyList<TGlyph> glyphs, TFont font, IEnumerable<PointF> points, Color? color) {
      if (color.HasValue) CgContext.SetFillColor(color.GetValueOrDefault().ToCGColor());
      font.CtFont.DrawGlyphs(CgContext, glyphs.ToArray(), points.Select(p => (CGPoint)p).ToArray());
    }

    public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness, Color? color) {
      CgContext.SetLineWidth(lineThickness);
      CgContext.SetLineCap(CGLineCap.Round);
      if (color.HasValue) CgContext.SetStrokeColor(color.GetValueOrDefault().ToCGColor());
      CgContext.AddLines(new[] { new CGPoint(x1, y1), new CGPoint(x2, y2) });
      CgContext.StrokePath();
    }

    public void DrawGlyphRunWithOffset(AttributedGlyphRun<TFont, TGlyph> run, PointF offset, Color? color) {
      CgContext.TextPosition = new CGPoint
        (CgContext.TextPosition.X + offset.X, CgContext.TextPosition.Y + offset.Y);
      if (color.HasValue) CgContext.SetFillColor(color.GetValueOrDefault().ToCGColor());
      using var textLine = new CTLine(run.ToNsAttributedString());
      textLine.Draw(CgContext);
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