using System.Diagnostics;
using System.Drawing;
using System.Linq;
using CoreGraphics;
using CoreText;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using CSharpMath.Structures;
using UIKit;
using TFont = CSharpMath.Apple.AppleMathFont;
using TGlyph = System.UInt16;

namespace CSharpMath.Apple.Drawing {
  public class AppleGraphicsContext : IGraphicsContext<TFont, TGlyph> {
    
    public CGContext CgContext { get; set; }

    public void DrawGlyphsAtPoints(TGlyph[] glyphs, TFont font, PointF[] points, Color? color)
    {
      DebugWriteLine(glyphs, points);
      var ctFont = font.CtFont;
      var cgPoints = points.Select(p => (CGPoint)p).ToArray();
      if(color.HasValue) CgContext.SetFillColor(color.Value.ToCgColor());
      ctFont.DrawGlyphs(CgContext, glyphs, cgPoints);
    }

    public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness, Color? color) {
      DebugWriteLine($"DrawLine {x1} {y1} {x2} {y2}");
      CgContext.SetLineWidth(lineThickness);
      CgContext.SetLineCap(CGLineCap.Round);
      if (color.HasValue) CgContext.SetStrokeColor(color.Value.ToCgColor());
      CgContext.AddLines(new[] { new CGPoint(x1, y1), new CGPoint(x2, y2) });
      CgContext.StrokePath();
    }

    public void DrawGlyphRunWithOffset(AttributedGlyphRun<TFont, TGlyph> run, PointF offset, Color? color) {
      DebugWriteLine($"Text {run} {offset.X} {offset.Y}");
      var attributedString = run.ToNsAttributedString();
      CgContext.TextPosition = new CGPoint(CgContext.TextPosition.X + offset.X, CgContext.TextPosition.Y + offset.Y);
      if (color.HasValue) CgContext.SetFillColor(color.Value.ToCgColor());
      using (var textLine = new CTLine(attributedString)) {
        textLine.Draw(CgContext);
      }
    }

    public void RestoreState() {
      DebugWriteLine($"Restore");
      CgContext.RestoreState();
    }

    public void SaveState() {
      DebugWriteLine($"Save");
      CgContext.SaveState();
    }

    public void SetTextPosition(PointF position) {
      DebugWriteLine($"SetTextPosition {position.X} {position.Y}");
      CgContext.TextPosition = position;
    }

    public void Translate(PointF dxy) {
      DebugWriteLine($"translate {dxy.X} {dxy.Y}");
      CgContext.TranslateCTM(dxy.X, dxy.Y);
    }

    [Conditional("DEBUG")]
    public void DebugWriteLine(System.FormattableString text) {
      Debug.WriteLine(text.ToString());
    }
    [Conditional("DEBUG")]
    public void DebugWriteLine(TGlyph[] glyphs, PointF[] points) {
      var glyphStrings = glyphs.Select(g => ((int)g).ToString()).ToArray();
      var pointStrings = points.Select(pt => $@"{pt.X} {pt.Y}").ToArray();
      for (int i = 0; i < glyphs.Count(); i++) {
        Debug.WriteLine($"    {glyphStrings[i]} {pointStrings[i]}");
      }
      Debug.WriteLine($"glyphs {glyphStrings} {pointStrings}");

    }
  }
}
