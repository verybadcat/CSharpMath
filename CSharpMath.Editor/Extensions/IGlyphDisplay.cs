namespace CSharpMath.Editor {
  using System;
  using System.Drawing;
  using Display;
  using Display.FrontEnd;

  partial class Extensions {
    public static MathListIndex IndexForPoint<TFont, TGlyph>(
      this IGlyphDisplay<TFont, TGlyph> self,
      TypesettingContext<TFont, TGlyph> _,
      PointF point) where TFont : IFont<TGlyph> =>
      point.X > self.Position.X + self.Width / 2
      ? MathListIndex.Level0Index(self.Range.End)
      : MathListIndex.Level0Index(self.Range.Location);

    public static PointF? PointForIndex<TFont, TGlyph>(
      this IGlyphDisplay<TFont, TGlyph> self,
      TypesettingContext<TFont, TGlyph> _,
      MathListIndex index) where TFont : IFont<TGlyph> =>
      index.SubIndexType != MathListSubIndexType.None
      ? throw new ArgumentException
        ("The subindex must be none to get the closest point for it.", nameof(index))
      : index.AtomIndex == self.Range.End
      // draw a caret after the glyph
      ? self.Position.Plus(new PointF(self.DisplayBounds().Right, 0))
      // draw a caret before the glyph
      : self.Position;

    public static void HighlightCharacterAt<TFont, TGlyph>(
      this IGlyphDisplay<TFont, TGlyph> self,
      MathListIndex index, Color color) where TFont : IFont<TGlyph> {
      if (index.SubIndexType != MathListSubIndexType.None)
        throw new ArgumentException
          ("The subindex must be none to get the highlight a character in it.", nameof(index));
      self.Highlight(color);
    }

    public static void Highlight<TFont, TGlyph>(
      this IGlyphDisplay<TFont, TGlyph> self, Color color) where TFont : IFont<TGlyph> {
      self.TextColor = color;
    }
  }
}
