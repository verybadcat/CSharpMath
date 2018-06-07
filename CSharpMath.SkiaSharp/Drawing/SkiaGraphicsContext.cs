using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSharpMath.FrontEnd;
using CSharpMath.Structures;
using TFont = CSharpMath.SkiaSharp.SkiaMathFont;
using SkiaSharp;
using Typography.OpenFont;

namespace CSharpMath.SkiaSharp {
  public class SkiaGraphicsContext : IGraphicsContext<TFont, Glyph> {
    protected readonly Stack<PointF> _posStack = new Stack<PointF>();
    public Stack<(SKPath path, SKPoint pos, SKColor? color)> Paths { get; } = new Stack<(SKPath, SKPoint, SKColor?)>();
    public Stack<(SKPath path, SKColor color)> BoxPaths { get; } = new Stack<(SKPath, SKColor)>();
    public Stack<(SKPoint from, SKPoint to, float thickness, SKColor? color)> LinePaths { get; } = new Stack<(SKPoint, SKPoint, float, SKColor?)>();
    public (SKColor glyph, SKColor textRun)? GlyphBoxColor { get; set; }

    public PointF DrawPosition { get; set; }
    public PointF TextPosition { get; set; }
    void IGraphicsContext<TFont, Glyph>.SetTextPosition(PointF position) {
      Debug($"TextPosition ({position.X}, {position.Y})");
      TextPosition = position;
    }

    public void DrawGlyphsAtPoints(Glyph[] glyphs, TFont font, PointF[] points, Color? color) {
      Debug($"Glyphs {string.Join("; ", glyphs.Zip(points, (g, p) => $"{g.GetCff1GlyphData().Name} ({p.X}, {p.Y})"))} ");
      var textPosition = DrawPosition.Plus(TextPosition);
      var typeface = font.Typeface;
      var pathBuilder = new SkiaGlyphPathBuilder(typeface);
      if (GlyphBoxColor != null) {
        var boxPath = new SKPath();
        var rects = new SkiaGlyphBoundsProvider().GetBoundingRectsForGlyphs(font, glyphs);
        for (int i = 0; i < rects.Length; i++) {
          var rect = rects[i];
          var point = points[i].Plus(textPosition);
          var _rect = new SKRect(point.X + rect.X, point.Y + rect.Y, point.X + rect.Right, point.Y + rect.Bottom);
          boxPath.AddRect(_rect);
        }
        BoxPaths.Push((boxPath, GlyphBoxColor.Value.glyph));
      }
      var scale = typeface.CalculateScaleToPixelFromPointSize(font.PointSize);
      var skColor = color.ToNative();
      for (int i = 0; i < glyphs.Length; i++) {
        var path = new SkiaGlyphPath();
        pathBuilder.BuildFromGlyph(glyphs[i], font.PointSize);
        pathBuilder.ReadShapes(path);
        Paths.Push((path.Path, new SKPoint(textPosition.X + points[i].X - (glyphs[i].Bounds.XMin - glyphs[i].GetOriginalBounds().XMin) * scale, textPosition.Y + points[i].Y), skColor));
      }
    }

    public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness, Color? color) {
      Debug($"Line ({x1}, {y1}) -> ({x2}, {y2})");
      LinePaths.Push((new SKPoint(x1 + DrawPosition.X, y1 + DrawPosition.Y), new SKPoint(x2 + DrawPosition.X, y2 + DrawPosition.Y), lineThickness, color.ToNative()));
    }

    public void DrawGlyphRunWithOffset(Display.Text.AttributedGlyphRun<TFont, Glyph> run, PointF offset, Color? color) {
      Debug($"Text {run.Text} ({offset.X}, {offset.Y})");
      var textPosition = DrawPosition.Plus(offset).Plus(TextPosition);

      if (GlyphBoxColor != null) {
        var box = run.Font.GlyphLayout.LayoutAndMeasureString(run.Text.ToCharArray(), 0, run.Text.Length, run.Font.PointSize);
        var _path = new SKPath();
        _path.AddRect(SKRect.Create(textPosition.X, textPosition.Y, box.width + run.KernedGlyphs.Sum(g => g.KernAfterGlyph), box.btbd));
        BoxPaths.Push((_path, GlyphBoxColor.Value.textRun));
      }

      var typeface = run.Font.Typeface;
      var glyphs = run.KernedGlyphs;
      var pointSize = run.Font.PointSize;
      var layout = run.Font.GlyphLayout;
      var pathBuilder = new SkiaGlyphPathBuilder(typeface);
      var scale = typeface.CalculateScaleToPixelFromPointSize(pointSize);
      var skColor = color.ToNative();
      for (int i = 0; i < glyphs.Length; i++) {
        var path = new SkiaGlyphPath();
        var index = glyphs[i].Glyph.GlyphIndex;
        pathBuilder.BuildFromGlyph(glyphs[i].Glyph, pointSize);
        pathBuilder.ReadShapes(path);
        Paths.Push((path.Path, new SKPoint(textPosition.X, textPosition.Y), skColor));
        textPosition.X += typeface.GetHAdvanceWidthFromGlyphIndex(index) * scale + glyphs[i].KernAfterGlyph;
      }
    }

    public void RestoreState() {
      Debug("Restore");
      DrawPosition = _posStack.Pop();
    }

    public void SaveState() {
      Debug("Save");
      _posStack.Push(DrawPosition);
    }

    public void Translate(PointF dxy) {
      Debug($"Translate ({dxy.X}, {dxy.Y})");
      DrawPosition = DrawPosition.Plus(dxy);
    }
    
    [System.Diagnostics.DebuggerStepThrough, System.Diagnostics.Conditional("DEBUG")]
    private void Debug(string message) {
      System.Diagnostics.Debug.WriteLine(message); //comment out to avoid spamming the debug output
    }
  }
}
