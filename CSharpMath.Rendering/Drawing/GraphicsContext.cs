using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSharpMath.FrontEnd;
using CSharpMath.Structures;
using TFonts = CSharpMath.Rendering.MathFonts;
using Typography.OpenFont;

namespace CSharpMath.Rendering {
  public class GraphicsContext : IGraphicsContext<TFonts, Glyph> {
    public (Color glyph, Color textRun)? GlyphBoxColor { get; set; }
    public ICanvas Canvas { get; set; }
    
    public PointF TextPosition { get; set; }
    void IGraphicsContext<TFont, Glyph>.SetTextPosition(PointF position) => TextPosition = position;

    public void DrawGlyphsAtPoints(Glyph[] glyphs, TFont font, PointF[] points, Color? color) {
      var textPosition = TextPosition;
      var typeface = font.Typeface;
      var pathBuilder = new GlyphPathBuilder(typeface);
      if (GlyphBoxColor != null) {
        var rects = new GlyphBoundsProvider().GetBoundingRectsForGlyphs(font, glyphs);
        for (int i = 0; i < rects.Length; i++) {
          var rect = rects[i];
          var point = points[i].Plus(textPosition);
          Canvas.CurrentColor = GlyphBoxColor?.glyph;
          Canvas.StrokeRect(point.X + rect.X, point.Y + rect.Y, rect.Width, rect.Height);
        }
      }
      var scale = typeface.CalculateScaleToPixelFromPointSize(font.PointSize);
      for (int i = 0; i < glyphs.Length; i++) {
        pathBuilder.BuildFromGlyph(glyphs[i], font.PointSize);
        Canvas.Save();
        Canvas.CurrentColor = color;
        Canvas.Translate(textPosition.X + points[i].X - (glyphs[i].Bounds.XMin - glyphs[i].GetOriginalBounds().XMin) * scale, textPosition.Y + points[i].Y);
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
        var box = run.Font.GlyphLayout.LayoutAndMeasureString(run.Text.ToCharArray(), 0, run.Text.Length, run.Font.PointSize);
        Canvas.CurrentColor = GlyphBoxColor?.textRun;
        Canvas.StrokeRect(textPosition.X, textPosition.Y, box.width + run.KernedGlyphs.Sum(g => g.KernAfterGlyph), box.btbd);
      }

      var typeface = run.Font.Typeface;
      var glyphs = run.KernedGlyphs;
      var pointSize = run.Font.PointSize;
      var layout = run.Font.GlyphLayout;
      var pathBuilder = new GlyphPathBuilder(typeface);
      var scale = typeface.CalculateScaleToPixelFromPointSize(pointSize);
      Canvas.Save();
      Canvas.Translate(textPosition.X, textPosition.Y);
      for (int i = 0; i < glyphs.Length; i++) {
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
