using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSharpMath.Display;
using CSharpMath.CoreTests.FrontEnd;

namespace CSharpMath.Editor.Tests.Visualizer {
  public class GraphicsContext : Display.FrontEnd.IGraphicsContext<TestFont, char> {
    readonly Stack<PointF> stack = new Stack<PointF>();
    PointF trans = new PointF();
    public void DrawGlyphRunWithOffset(AttributedGlyphRun<TestFont, char> text,
      PointF point, Structures.Color? color) {
      var advance = 0.0;
      foreach (var ((glyph, kernAfter, foreground), bounds) in
        text.GlyphInfos.Zip(
          TestTypesettingContexts.Instance.GlyphBoundsProvider
          .GetBoundingRectsForGlyphs(text.Font, text.Glyphs, text.Length
        ), ValueTuple.Create)) {
        Checker.ConsoleDrawRectangle(
          new Rectangle((int)(point.X + trans.X + advance),
            (int)(point.Y + trans.Y), (int)bounds.Width, (int)bounds.Height),
          glyph, foreground ?? color);
        advance += bounds.Width + kernAfter;
      }
    }
    public void DrawGlyphsAtPoints(IReadOnlyList<char> glyphs,
      TestFont font, IEnumerable<PointF> points, Structures.Color? color) {
      var zipped = glyphs.Zip(points, ValueTuple.Create);
      var bounds = TestTypesettingContexts.Instance.GlyphBoundsProvider
        .GetBoundingRectsForGlyphs(font, glyphs, glyphs.Count);
      foreach (var ((glyph, point), bound) in zipped.Zip(bounds, ValueTuple.Create)) {
        Checker.ConsoleDrawRectangle(
          new Rectangle((int)(point.X + trans.X), (int)(point.Y + trans.Y),
            (int)bound.Width, (int)bound.Height),
          glyph, color
        );
      }
    }
    public void FillRect(Rectangle rect, Structures.Color color) =>
      Checker.ConsoleFillRectangle(new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height), color);
    public void DrawLine
      (float x1, float y1, float x2, float y2, float strokeWidth, Structures.Color? color) {
      if (y1 != y2) throw new NotImplementedException("Non-horizontal lines currently not supported");
      if (!Checker.OutputLines) return;
      Checker.ConsoleDrawHorizontal((int)(x1 + trans.X), (int)(y1 + trans.Y), (int)(x2 + trans.X),
        (int)MathF.Round(strokeWidth) /* e.g. for \frac, strokeWidth = 0.8 */, color);
    }
    public void RestoreState() => trans = stack.Pop();
    public void SaveState() => stack.Push(trans);
    public void SetTextPosition(PointF position) => trans = trans.Plus(position);
    public void Translate(PointF dxy) => trans = trans.Plus(dxy);
  }
}
