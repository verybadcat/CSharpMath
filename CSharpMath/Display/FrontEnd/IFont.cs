using System.Collections.Generic;
using System.Drawing;

namespace CSharpMath.Display.FrontEnd {
  public interface IFont<TGlyph> { float PointSize { get; } }

  /// <summary>Optional font capability used to determine painted (ink) extents.
  /// Layout continues to work for fonts which only provide advances.</summary>
  public interface IFontGlyphBounds<TGlyph> {
    IEnumerable<RectangleF> GetBoundingRects(IEnumerable<TGlyph> glyphs);
    IEnumerable<float> GetAdvances(IEnumerable<TGlyph> glyphs);
  }
}
