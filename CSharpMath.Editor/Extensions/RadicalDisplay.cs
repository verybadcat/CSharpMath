namespace CSharpMath.Editor {
  using System;
  using System.Drawing;
  using Display.Displays;
  using Display.FrontEnd;

  partial class Extensions {
    public static MathListIndex IndexForPoint<TFont, TGlyph>(
      this RadicalDisplay<TFont, TGlyph> self,
      TypesettingContext<TFont, TGlyph> context,
      PointF point) where TFont : IFont<TGlyph> =>
      // We can be before or after the radical
      point.X < self.Position.X - PixelDelta
      //We are before the radical, so
      ? MathListIndex.Level0Index(self.Range.Location)
      : point.X > self.Position.X + self.Width + PixelDelta
      //We are after the radical
      ? MathListIndex.Level0Index(self.Range.End)
      //We can be either near the degree or the radicand
      : DistanceFromPointToRect(point, self.Degree != null ? new RectangleF(self.Degree.Position, self.Degree.DisplayBounds().Size) : default)
      < DistanceFromPointToRect(point, new RectangleF(self.Radicand.Position, self.Radicand.DisplayBounds().Size))
      ? self.Degree != null
        ? MathListIndex.IndexAtLocation(self.Range.Location, MathListSubIndexType.Degree, self.Degree.IndexForPoint(context, point))
        : MathListIndex.Level0Index(self.Range.Location)
      : MathListIndex.IndexAtLocation(self.Range.Location, MathListSubIndexType.Radicand, self.Radicand.IndexForPoint(context, point));
    
    public static PointF? PointForIndex<TFont, TGlyph>(
      this RadicalDisplay<TFont, TGlyph> self,
      TypesettingContext<TFont, TGlyph> _,
      MathListIndex index) where TFont : IFont<TGlyph> {
      if (index.SubIndexType != MathListSubIndexType.None)
        throw new ArgumentException("The subindex must be none to get the closest point for it.", nameof(index));

      if (index.AtomIndex == self.Range.End)
        // draw a caret after the radical
        return self.Position.Plus(new PointF(self.DisplayBounds().Right, 0));
      // draw a caret before the radical
      return self.Position;
    }

    public static void HighlightCharacterAt<TFont, TGlyph>(
      this RadicalDisplay<TFont, TGlyph> self,
      MathListIndex index,
      Color color) where TFont : IFont<TGlyph> {
      if (index.SubIndexType != MathListSubIndexType.None)
        throw new ArgumentException("The subindex must be none to get the highlight a character in it.", nameof(index));
      self.Highlight(color);
    }

    public static void Highlight<TFont, TGlyph>(
      this RadicalDisplay<TFont, TGlyph> self,
      Color color) where TFont : IFont<TGlyph> {
      self.Degree?.Highlight(color);
      self.Radicand.Highlight(color);
    }
  }
}
