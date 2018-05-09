using SkiaSharp;
using System.Drawing;
using Typography.OpenFont;
using TFont = CSharpMath.SkiaSharp.SkiaMathFont;
using CSharpMath.FrontEnd;
using System.Linq;
using CSharpMath.Display.Text;

namespace CSharpMath.SkiaSharp {
  public class SkiaGraphicsContext : IGraphicsContext<TFont, Glyph> {
    protected SKPaint glyphPaint =
      new SKPaint { IsStroke = true, StrokeCap = SKStrokeCap.Round, StrokeWidth = 2 };
    
    public SKColor Color { get => glyphPaint.Color; set => glyphPaint.Color = value; }

    private PointF textPosition;
    public PointF TextPosition {
      get => textPosition;
      set {
        textPosition = value;
        Debug("SetTextPosition " + textPosition.X + " " + textPosition.Y);
      }
    }
    void IGraphicsContext<TFont, Glyph>.SetTextPosition(PointF position) => TextPosition = position;

    public SKCanvas Canvas { get; set; }

    public void DrawGlyphsAtPoints(Glyph[] glyphs, TFont font, PointF[] points) {
      Debug($"glyphs {string.Join(" ", glyphs.Select(g => g.ToString()).ToArray())}");
      var typeface = font.Typeface;

      //Modified version of https://github.com/LayoutFarm/Typography/blob/master/Demo/Windows/GlyphTess.WinForms/Form1.cs
      var pathBuilder = new Typography.Contours.GlyphPathBuilder(typeface);
      var translator = new DrawingGL.Text.GlyphTranslatorToPath();
      var path = new DrawingGL.WritablePath();
      translator.SetOutput(path);
      var curveFlattener = new DrawingGL.SimpleCurveFlattener();
      for (int i = 0; i < glyphs.Length; i++) {
        pathBuilder.BuildFromGlyphIndex(glyphs[i].GetCff1GlyphData().GlyphIndex, font.PointSize);
        pathBuilder.ReadShapes(translator);
        var flattenPoints = curveFlattener.Flatten(path._points, out var contourEnds);

        int contourCount = contourEnds.Length;
        int startAt = 3;
        for (int cnt_index = 0; cnt_index < contourCount; ++cnt_index) {
          int endAt = contourEnds[cnt_index];
          SaveState();
          Translate(points[i]);
          for (int m = startAt; m <= endAt; m += 2) {
            Canvas.DrawLine(flattenPoints[m - 3], flattenPoints[m - 2], flattenPoints[m - 1], flattenPoints[m], glyphPaint);

          }
          //close coutour 
          Canvas.DrawLine(flattenPoints[endAt - 1], flattenPoints[endAt], flattenPoints[startAt - 3], flattenPoints[startAt - 2], glyphPaint);
          RestoreState();
          //
          startAt = (endAt + 1) + 3;
        }
      }
    }

    public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness) {
      Debug($"DrawLine {x1} {y1} {x2} {y2}");
      Canvas.DrawLine(x1, y1, x2, y2, new SKPaint { IsStroke = true, StrokeCap = SKStrokeCap.Round, StrokeWidth = lineThickness });
    }

    public void DrawGlyphRunWithOffset(AttributedGlyphRun<TFont, Glyph> run, PointF offset, float maxWidth = float.NaN) {
      Debug($"Text {run} {offset.X} {offset.Y}");
      textPosition = textPosition.Plus(offset);
      
      var layout = run.Font.GlyphLayout;
      layout.Layout(run.Text.ToCharArray(), 0, run.Length);

      var totalAdvance = textPosition.X;
      var glyphPositions = new PointF[layout._glyphPositions.Count];
      for (int i = 0; i < glyphPositions.Length; i++) {
        var p = layout._glyphPositions[i];
        var glyphPosition = new PointF(totalAdvance + p.OffsetX, textPosition.Y + p.OffsetY);
        totalAdvance += p.advanceW;
        glyphPositions[i] = glyphPosition;
      }
      DrawGlyphsAtPoints(run.Glyphs, run.Font, glyphPositions);

      //GlyphPlanList glyphPlans = new GlyphPlanList();
      //GlyphLayoutExtensions.GenerateGlyphPlan(layout._glyphPositions, layout.Typeface.CalculateScaleToPixelFromPointSize(run.Font.PointSize), false, glyphPlans);
      //float totalKern = 0f;
      //DrawGlyphsAtPoints(run.Glyphs, run.Font,
      //  glyphPlans.Zip(/*kern before each glyph*/ new[] { 0f }.Concat(run.KernedGlyphs.Select(g => g.KernAfterGlyph)),
      //    (g, k) => new PointF(g.ExactX + (/*take into account of aggregated kern*/ totalKern += k), g.ExactY).Plus(textPosition)).ToArray());
    }

    public void RestoreState() {
      Debug("Restore");
      Canvas.Restore();
    }

    public void SaveState() {
      Debug("Save");
      Canvas.Save();
    }

    public void Translate(PointF dxy) {
      Debug("translate " + dxy.X + " " + dxy.Y);
      Canvas.Translate(dxy.X, dxy.Y);
    }
    
    private void Debug(string message) {
      //System.Diagnostics.Debug.WriteLine(message); //comment out to avoid spamming the debug output
    }
  }
}
