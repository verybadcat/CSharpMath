using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display {
  /// <summary>Represents a part of a glyph used for constructing a large vertical or horizontal glyph.</summary>
  public class GlyphPart<TGlyph> {
    public TGlyph Glyph { get; set; }
    public float FullAdvance { get; set; }
    public float StartConnectorLength { get; set; }
    public float EndConnectorLength { get; set; }

    /// <summary>If the glyph is an extender, it can be skipped or repeated.</summary>
    public bool IsExtender { get; set; }

    public override string ToString()
    {
      return string.Format("[GlyphPart: Glyph={0}, FullAdvance={1}, StartConnectorLength={2}, EndConnectorLength={3}, IsExtender={4}]", Glyph, FullAdvance, StartConnectorLength, EndConnectorLength, IsExtender);
    }
  }
}
