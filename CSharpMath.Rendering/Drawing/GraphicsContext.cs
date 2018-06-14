using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSharpMath.FrontEnd;
using CSharpMath.Structures;
using TFonts = CSharpMath.Rendering.MathFonts;
using Typography.OpenFont;
using Typography.TextLayout;

namespace CSharpMath.Rendering {
  public class GraphicsContext : IGraphicsContext<TFonts, Glyph> {
    public (Color glyph, Color textRun)? GlyphBoxColor { get; set; }
    public ICanvas Canvas { get; set; }
    
    public PointF TextPosition { get; set; }
    void IGraphicsContext<TFont, Glyph>.SetTextPosition(PointF position) => TextPosition = position;

    public void DrawGlyphsAtPoints(Glyph[] glyphs, TFont font, PointF[] points, Color? color) {
      var textPosition = TextPosition;
      if (GlyphBoxColor != null) {
        var rects = new GlyphBoundsProvider().GetBoundingRectsForGlyphs(font, glyphs);
        for (int i = 0; i < rects.Length; i++) {
          var rect = rects[i];
          var point = points[i].Plus(textPosition);
          Canvas.CurrentColor = GlyphBoxColor?.glyph;
          Canvas.StrokeRect(point.X + rect.X, point.Y + rect.Y, rect.Width, rect.Height);
        }
      }
      for (int i = 0; i < glyphs.Length; i++) {
        var typeface = glyphs[i].Typeface;
        var scale = typeface.CalculateScaleToPixelFromPointSize(font.PointSize);
        var pathBuilder = new GlyphPathBuilder(typeface);
        pathBuilder.BuildFromGlyph(glyphs[i].Info, font.PointSize);
        Canvas.Save();
        Canvas.CurrentColor = color;
        Canvas.Translate(textPosition.X + points[i].X - (glyphs[i].Info.Bounds.XMin - glyphs[i].GetOriginalBounds().XMin) * scale, textPosition.Y + points[i].Y);
        pathBuilder.ReadShapes(Canvas);
        Canvas.Restore();
      }
    }

    public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness, Color? color) {
      Canvas.CurrentColor = color;
      Canvas.AddLine(x1, y1, x2, y2, lineThickness);
    }

    public void DrawGlyphRunWithOffset(Display.Text.AttributedGlyphRun<TFont, Glyph> run, PointF offset, Color? color) {
      var textPosition = offset.Plus(TextPosition);

      if (GlyphBoxColor != null) {
        var size = new SizeF();
        Bounds bounds;
        foreach (var glyph in run.KernedGlyphs) {
          bounds = glyph.Glyph.Info.Bounds;
          size.Width += bounds.XMax - bounds.XMin;
          size.Height += bounds.YMax - bounds.YMin;
        }
        Canvas.CurrentColor = GlyphBoxColor?.textRun;
        Canvas.StrokeRect(textPosition.X, textPosition.Y, size.Width + run.KernedGlyphs.Sum(g => g.KernAfterGlyph), size.Height);
      }

      var glyphs = run.KernedGlyphs;
      var pointSize = run.Font.PointSize;
      Canvas.Save();
      Canvas.Translate(textPosition.X, textPosition.Y);
      for (int i = 0; i < glyphs.Length; i++) {
        var typeface = glyphs[i].Glyph.Typeface;
        var layout = new GlyphLayout { Typeface = typeface };
        var pathBuilder = new GlyphPathBuilder(typeface);
        var scale = typeface.CalculateScaleToPixelFromPointSize(pointSize);
        var index = glyphs[i].Glyph.Info.GlyphIndex;
        pathBuilder.BuildFromGlyph(glyphs[i].Glyph.Info, pointSize);
        Canvas.CurrentColor = color;
        pathBuilder.ReadShapes(Canvas);
        Canvas.Translate(typeface.GetHAdvanceWidthFromGlyphIndex(index) * scale + glyphs[i].KernAfterGlyph, 0);
      }
      Canvas.Restore();
    }

    public void RestoreState() => Canvas.Restore();

    public void SaveState() => Canvas.Save();

    public void Translate(PointF dxy) => Canvas.Translate(dxy.X, dxy.Y);
  }
}
