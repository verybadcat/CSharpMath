namespace CSharpMath.Display {
  /// <summary>Represents a part of a glyph used for constructing a large vertical or horizontal glyph.</summary>
  public class GlyphPart<TGlyph> {
    public GlyphPart(TGlyph glyph,
                     float fullAdvance,
                     float startConnectorLength,
                     float endConnectorLength,
                     bool isExtender) {
      Glyph = glyph;
      FullAdvance = fullAdvance;
      StartConnectorLength = startConnectorLength;
      EndConnectorLength = endConnectorLength;
      IsExtender = isExtender;
    }
    public TGlyph Glyph { get; }
    public float FullAdvance { get; }
    public float StartConnectorLength { get; }
    public float EndConnectorLength { get; }
    /// <summary>If the glyph is an extender, it can be skipped or repeated.</summary>
    public bool IsExtender { get; }
    public override string ToString() =>
      $@"[{nameof(GlyphPart<TGlyph>)}: {nameof(Glyph)}={Glyph}, {nameof(FullAdvance)}={FullAdvance}, {nameof(StartConnectorLength)}={StartConnectorLength}, {nameof(EndConnectorLength)}={EndConnectorLength}, {nameof(IsExtender)}={IsExtender}]";
  }
}