using System.Diagnostics;
using System.Drawing;
using System.Linq;
using CoreGraphics;
using CoreText;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using Color = CSharpMath.Structures.Color;
using UIKit;
using TFont = CSharpMath.Apple.AppleMathFont;
using TGlyph = System.UInt16;

namespace CSharpMath.Apple.Drawing {
  public class AppleGraphicsContext : IGraphicsContext<TFont, TGlyph> {
    
    public CGContext CgContext { get; set; }

    public void DrawGlyphsAtPoints(ForEach<TGlyph> glyphs, TFont font, ForEach<PointF> points, Color? color)
    {
      DebugWriteLine(glyphs, points);
      var cgPoints = points.Select(p => (CGPoint)p);
      var cgArray = new RentedArray<CGPoint>(cgPoints.Count);
      cgPoints.CopyTo(cgArray.Array);
      var glyphArray = new RentedArray<TGlyph>(cgPoints.Count);
      glyphs.CopyTo(glyphArray.Array);
      var ctFont = font.CtFont;
      if(color.HasValue) CgContext.SetFillColor(color.Value.ToCgColor());
      ctFont.DrawGlyphs(CgContext, glyphArray.Array, cgArray.Array);
      cgArray.Return();
      glyphArray.Return();
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
      CgContext.TextPosition = new CGPoint(CgContext.TextPosition.X + offset.X, CgContext.TextPosition.Y + offset.Y);
      if (color.HasValue) CgContext.SetFillColor(color.Value.ToCgColor());
      using (var textLine = new CTLine(run.ToNsAttributedString()))
        textLine.Draw(CgContext);
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
    public void DebugWriteLine(ForEach<TGlyph> glyphs, ForEach<PointF> points) {
      var glyphStrings = glyphs.Select(g => ((int)g).ToString());
      var pointStrings = points.Select(pt => $@"{pt.X} {pt.Y}");
      for (int i = 0; i < glyphStrings.Count; i++)
        Debug.WriteLine($"    {glyphStrings[i]} {pointStrings[i]}");
      Debug.WriteLine($"glyphs {glyphStrings} {pointStrings}");
    }
  }
}
