using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CSharpMath.Display.Text;
using CSharpMath.Structures;
using CSharpMath.Tests.FrontEnd;

namespace CSharpMath.Editor.TestChecker {
  public class GraphicsContext : FrontEnd.IGraphicsContext<TestFont, char> {
    PointF Invert(PointF p) => new PointF(p.X, -p.Y);

    readonly Stack<PointF> stack = new Stack<PointF>();
    PointF trans = new PointF();

    public void DrawGlyphRunWithOffset(AttributedGlyphRun<TestFont, char> text, PointF point, Structures.Color? color) {
      point = Invert(point);
      var advance = 0.0;
      foreach (var (glyph, bounds) in text.GlyphInfos.Zip(
        TestTypesettingContexts.Instance.GlyphBoundsProvider.GetBoundingRectsForGlyphs(text.Font, text.Glyphs.AsForEach(), text.Length),
        ValueTuple.Create)) {
        Checker.ConsoleDrawRectangle((int)bounds.Width, (int)bounds.Height, new Point((int)(point.X + trans.X + advance), (int)(point.Y + trans.Y)), glyph.Glyph, glyph.Foreground);
        advance += bounds.Width + glyph.KernAfterGlyph;
      }
    }

    public void DrawGlyphsAtPoints(ForEach<char> glyphs, TestFont font, ForEach<PointF> points, Structures.Color? color) {
      var zipped = glyphs.Zip(points);
      var bounds = TestTypesettingContexts.Instance.GlyphBoundsProvider.GetBoundingRectsForGlyphs(font, glyphs, zipped.Count);
      foreach (var ((glyph, point_), bound) in zipped.Zip(bounds, ValueTuple.Create)) {
        var point = Invert(point_);
        Checker.ConsoleDrawRectangle((int)bound.Width, (int)bound.Height, new Point((int)(point.X + trans.X), (int)(point.Y + trans.Y)), glyph, color);
      }
    }

#warning Needs implementation
    public void DrawLine(float x1, float y1, float x2, float y2, float strokeWidth, Structures.Color? color) { } // Nothing for now

    public void RestoreState() => trans = stack.Pop();
    public void SaveState() => stack.Push(trans);

    public void SetTextPosition(PointF position) => trans = trans.Plus(Invert(position));
    public void Translate(PointF dxy) => trans = trans.Plus(Invert(dxy));
  }
}