using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using Typography.OpenFont;
using Typography.TextLayout;
using TFont = CSharpMath.SkiaSharp.SkiaMathFont;
using TGlyph = System.Int32;
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
      Debug.WriteLine($"glyphs {string.Join(" ", glyphs.Select(g => g.ToString()).ToArray())}");
      var typeface = font.Typeface;
      var skPoints = points.Select(p => new SKPoint(p.X, p.Y)).ToArray();
      foreach (var glyph in glyphs) {
        typeface.GetGlyphByIndex(glyph).
      }
      ctFont.DrawGlyphs(Canvas, glyphs, cgPoints);
    }

    public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness) {
      Debug.WriteLine($"DrawLine {x1} {y1} {x2} {y2}");
      Canvas.DrawLine(x1, y1, x2, y2, new SKPaint { StrokeCap = SKStrokeCap.Round, StrokeWidth = lineThickness });
    }

    public void DrawGlyphRunWithOffset(AttributedGlyphRun<TFont, TGlyph> run, PointF offset, float maxWidth = float.NaN) {
      Debug.WriteLine($"Text {run} {offset.X} {offset.Y}");
      textPosition = textPosition.Plus(offset);
      
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
