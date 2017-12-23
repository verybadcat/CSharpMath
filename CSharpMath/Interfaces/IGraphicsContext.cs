using System;
using System.Drawing;
using CSharpMath.Display;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;

namespace CSharpMath {
  /// <summary>Represents a front-end graphics context.  NOT related to IContext, CalculatorContext, etc.</summary>
  public interface IGraphicsContext<TFont, TGlyph>
    where TFont: MathFont<TGlyph> {
    void DrawLine(float x1, float y1, float x2, float y2, float strokeWidth);
    void DrawGlyphRunWithOffset(AttributedGlyphRun<TFont, TGlyph> text, PointF point, float maxWidth = float.NaN);
    void DrawGlyphsAtPoints(TGlyph[] glyph, TFont font, PointF[] points);
    void Translate(PointF dxy);
    void SaveState();
    void RestoreState();
    void SetTextPosition(PointF position);
    IGlyphFinder<TGlyph> GlyphFinder { get; }
  }

}
