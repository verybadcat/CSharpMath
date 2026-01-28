using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSharpMath.Display;
using TGlyph = System.Text.Rune;

namespace CSharpMath.Core {
  using BackEnd;
  public class GraphicsContext : Display.FrontEnd.IGraphicsContext<TestFont, TGlyph> {
    readonly Stack<PointF> stack = new Stack<PointF>();
    PointF trans = new PointF();
    public void DrawGlyphRunWithOffset(AttributedGlyphRun<TestFont, TGlyph> text,
      PointF point, Color? color) {
      var advance = 0.0;
      foreach (var ((glyph, kernAfter, foreground), bounds) in
        text.GlyphInfos.Zip(
          TestTypesettingContext.Instance.GlyphBoundsProvider
          .GetBoundingRectsForGlyphs(text.Font, text.Glyphs, text.Length
        ), ValueTuple.Create)) {
        Checker.ConsoleDrawRectangle(
          new Rectangle((int)(point.X + trans.X + bounds.X + advance),
            (int)(point.Y + trans.Y + bounds.Y), (int)bounds.Width, (int)bounds.Height),
          glyph, foreground ?? color);
        advance += bounds.Width + kernAfter;
      }
    }
    public void DrawGlyphsAtPoints(IReadOnlyList<TGlyph> glyphs,
      TestFont font, IEnumerable<PointF> points, Color? color) {
      var zipped = glyphs.Zip(points, ValueTuple.Create);
      var bounds = TestTypesettingContext.Instance.GlyphBoundsProvider
        .GetBoundingRectsForGlyphs(font, glyphs, glyphs.Count);
      foreach (var ((glyph, point), bound) in zipped.Zip(bounds, ValueTuple.Create)) {
        Checker.ConsoleDrawRectangle(
          new Rectangle((int)(point.X + trans.X + bound.X), (int)(point.Y + trans.Y + bound.Y),
            (int)bound.Width, (int)bound.Height),
          glyph, color
        );
      }
    }
    public void FillRect(RectangleF rect, Color color) =>
      Checker.ConsoleFillRectangle(new Rectangle((int)(rect.X + trans.X), (int)(rect.Y + trans.Y), (int)rect.Width, (int)rect.Height), color);
    public void DrawLine
      (float x1, float y1, float x2, float y2, float strokeWidth, Color? color) {
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