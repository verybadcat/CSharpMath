using System.Collections.Generic;
using System.Drawing;

namespace CSharpMath.Display.FrontEnd {
  /// <summary>Represents a front-end graphics context. Used for drawing.</summary>
  public interface IGraphicsContext<TFont, TGlyph> where TFont: IFont<TGlyph> {
    void DrawLine(float x1, float y1, float x2, float y2, float strokeWidth, Color? color);
    void DrawGlyphRunWithOffset
      (AttributedGlyphRun<TFont, TGlyph> text, PointF point, Color? color);
    void DrawGlyphsAtPoints
      (IReadOnlyList<TGlyph> glyphs, TFont font, IEnumerable<PointF> points, Color? color);
    void FillRect(RectangleF rect, Color color);
    void Translate(PointF dxy);
    void SaveState();
    void RestoreState();
    void SetTextPosition(PointF position);
  }
}