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

    public override string ToString() =>
      $@"[{nameof(GlyphPart<TGlyph>)}: {nameof(Glyph)}={Glyph}, {nameof(FullAdvance)}={FullAdvance}, {nameof(StartConnectorLength)}={StartConnectorLength}, {nameof(EndConnectorLength)}={EndConnectorLength}, {nameof(IsExtender)}={IsExtender}]";
  }
}
