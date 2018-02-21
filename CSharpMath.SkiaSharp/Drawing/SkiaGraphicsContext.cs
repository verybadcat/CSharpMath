using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using Typography.OpenFont;
using Typography.TextLayout;
using TFont = CSharpMath.SkiaSharp.SkiaMathFont;
using TGlyph = System.UInt16;
using CSharpMath.FrontEnd;
using System.Linq;
using CSharpMath.Display.Text;

namespace CSharpMath.SkiaSharp.Drawing {
  public class SkiaGraphicsContext : IGraphicsContext<TFont, TGlyph> {
    private static readonly SKPaint glyphPaint =
      new SKPaint { IsStroke = true, StrokeCap = SKStrokeCap.Round, StrokeWidth = 2 };

    private PointF textPosition;

    public SKCanvas Canvas { get; set; }

    public void DrawGlyphsAtPoints(TGlyph[] glyphs, TFont font, PointF[] points) {
      Debug($"glyphs {string.Join(" ", glyphs.Select(g => g.ToString()).ToArray())}");
      var typeface = font.Typeface;

      //Modified version of https://github.com/LayoutFarm/Typography/blob/master/Demo/Windows/GlyphTess.WinForms/Form1.cs
      var pathBuilder = new Typography.Rendering.GlyphPathBuilder(typeface);
      var translator = new DrawingGL.Text.GlyphTranslatorToPath();
      var path = new DrawingGL.WritablePath();
      translator.SetOutput(path);
      var curveFlattener = new DrawingGL.SimpleCurveFlattener();
      for (int i = 0; i < glyphs.Length; i++) {
        pathBuilder.BuildFromGlyphIndex(glyphs[i], font.PointSize);
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

    public void DrawGlyphRunWithOffset(AttributedGlyphRun<TFont, TGlyph> run, PointF offset, float maxWidth = float.NaN) {
      Debug($"Text {run} {offset.X} {offset.Y}");
      textPosition = textPosition.Plus(offset);
      List<GlyphPlan> glyphPlans = null;
      run.Font.GlyphLayout.Layout(run.Text.ToCharArray(), 0, run.Length, glyphPlans);

      float totalKern = 0f;
      DrawGlyphsAtPoints(run.Glyphs, run.Font,
        glyphPlans.Zip(/*kern before each glyph*/ new[] { 0f }.Concat(run.KernedGlyphs.Select(g => g.KernAfterGlyph)),
          (g, k) => new PointF(g.ExactX + (/*take into account of aggregated kern*/ totalKern += k), g.ExactY).Plus(textPosition)).ToArray());
    }

    public void RestoreState() {
      Debug("Restore");
      Canvas.Restore();
    }

    public void SaveState() {
      Debug("Save");
      Canvas.Save();
    }

    public void SetTextPosition(PointF position) {
      Debug("SetTextPosition " + position.X + " " + position.Y);
      textPosition = position;
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
