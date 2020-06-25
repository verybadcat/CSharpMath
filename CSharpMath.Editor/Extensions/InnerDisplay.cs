namespace CSharpMath.Editor {
  using System;
  using System.Drawing;
  using Display.Displays;
  using Display.FrontEnd;

  partial class Extensions {
    public static MathListIndex IndexForPoint<TFont, TGlyph>(
      this InnerDisplay<TFont, TGlyph> self,
      TypesettingContext<TFont, TGlyph> context,
      PointF point) where TFont : IFont<TGlyph> =>
      // We can be before or after the inner
      point.X < self.Position.X + (self.Left?.Width / 2 ?? 0)
      //We are before the inner, so
      ? MathListIndex.Level0Index(self.Range.Location)
      : point.X > self.Position.X + self.Width - (self.Right?.Width / 2 ?? 0)
      //We are after the inner
      ? MathListIndex.Level0Index(self.Range.End)
      : MathListIndex.IndexAtLocation(self.Range.Location,
          MathListSubIndexType.Inner, self.Inner.IndexForPoint(context, point));

    public static PointF? PointForIndex<TFont, TGlyph>(
      this InnerDisplay<TFont, TGlyph> self,
      TypesettingContext<TFont, TGlyph> _,
      MathListIndex index) where TFont : IFont<TGlyph> =>
      index.SubIndexType != MathListSubIndexType.None
      ? throw new ArgumentException
        ("The subindex must be none to get the closest point for it.", nameof(index))
      : index.AtomIndex == self.Range.End
      // draw a caret after the inner
      ? self.Position.Plus(new PointF(self.DisplayBounds().Right, 0))
      // draw a caret before the inner
      : self.Position;

    public static void HighlightCharacterAt<TFont, TGlyph>(
      this InnerDisplay<TFont, TGlyph> self,
      MathListIndex index, Color color) where TFont : IFont<TGlyph> {
      if (index.SubIndexType != MathListSubIndexType.None)
        throw new ArgumentException
          ("The subindex must be none to get the highlight a character in it.", nameof(index));
      self.Highlight(color);
    }

    public static void Highlight<TFont, TGlyph>(
      this InnerDisplay<TFont, TGlyph> self, Color color) where TFont : IFont<TGlyph> =>
      self.Inner.Highlight(color);
  }
}
