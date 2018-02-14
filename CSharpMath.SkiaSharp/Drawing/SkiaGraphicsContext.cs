using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TGlyph = System.UInt16;
using TFont = CSharpMath.Apple.SkiaMathFont;
using CSharpMath.FrontEnd;
using System.Diagnostics;
using System.Linq;
using CSharpMath.Display.Text;

namespace CSharpMath.SkiaSharp.Drawing {
  public class SkiaGraphicsContext : IGraphicsContext<TFont, TGlyph> {

    public SkiaGraphicsContext(IGlyphFinder<TGlyph> glyphFinder) {
      GlyphFinder = glyphFinder;
    }

    private PointF textPosition;

    public SKCanvas Canvas { get; set; }

    public IGlyphFinder<TGlyph> GlyphFinder { get; set; }

    public void DrawGlyphsAtPoints(TGlyph[] glyphs, TFont font, PointF[] points)
    {
      var glyphStrings = string.Join(" ", glyphs.Select(g => ((int)g).ToString()).ToArray());
      Debug.WriteLine($"glyphs {glyphStrings}");
      var ctFont = font.CtFont;
      var cgPoints = points.Select(p => (CGPoint)p).ToArray();
      ctFont.DrawGlyphs(Canvas, glyphs, cgPoints);
    }

    public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness) {
      Debug.WriteLine($"DrawLine {x1} {y1} {x2} {y2}");
      UIBezierPath path = new UIBezierPath();
      path.LineWidth = lineThickness;
      path.LineCapStyle = CGLineCap.Round;
      path.MoveTo(new CGPoint(x1, y1));
      path.AddLineTo(new CGPoint(x2, y2));
      path.Stroke();
    }

    public void DrawGlyphRunWithOffset(AttributedGlyphRun<TFont, TGlyph> run, PointF offset, float maxWidth = float.NaN) {
      Debug.WriteLine($"Text {run} {offset.X} {offset.Y}");
      textPosition = textPosition.Plus(offset);

      run.
      using (var textLine = new CTLine(attributedString)) {
        textLine.Draw(Canvas);
      }
    }

    public void RestoreState() {
      Debug.WriteLine("Restore");
      Canvas.Restore();
    }

    public void SaveState() {
      Debug.WriteLine("Save");
      Canvas.Save();
    }

    public void SetTextPosition(PointF position) {
      Debug.WriteLine("SetTextPosition " + position.X + " " + position.Y);
      textPosition = position;
    }

    public void Translate(PointF dxy) {
      Debug.WriteLine("translate " + dxy.X + " " + dxy.Y);
      Canvas.Translate(dxy.X, dxy.Y);
    }


  }
}
