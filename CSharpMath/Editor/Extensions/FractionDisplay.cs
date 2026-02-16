namespace CSharpMath.Editor {
  using System;
  using System.Drawing;
  using Display.Displays;
  using Display.FrontEnd;

  partial class Extensions {
    public static MathListIndex? IndexForPoint<TFont, TGlyph>(
      this FractionDisplay<TFont, TGlyph> self,
      TypesettingContext<TFont, TGlyph> context,
      PointF point) where TFont : IFont<TGlyph> =>
      // We can be before or after the fraction
      point.X < self.Position.X - PixelDelta
      // We are before the fraction
      ? new(self.Range.Location)
      : point.X > self.Position.X + self.Width + PixelDelta
      // We are after the fraction
      ? new(self.Range.End)
      : point.Y > self.LinePosition + PixelDelta
      ? self.Numerator.IndexForPoint(context, point)?.WrapInIndex(self.Range.Location, MathListSubIndexType.Numerator)
      : point.Y < self.LinePosition - PixelDelta
      ? self.Denominator.IndexForPoint(context, point)?.WrapInIndex(self.Range.Location, MathListSubIndexType.Denominator)
      : point.X > self.Position.X + self.Width / 2
      ? new(self.Range.End)
      : new(self.Range.Location);

    public static PointF? PointForIndex<TFont, TGlyph>(
      this FractionDisplay<TFont, TGlyph> self,
      TypesettingContext<TFont, TGlyph> _,
      MathListIndex index) where TFont : IFont<TGlyph> =>
      index.SubIndexInfo is { }
      ? throw new ArgumentException
        ("The subindex must be none to get the closest point for it.", nameof(index))
      : index.AtomIndex == self.Range.End
      // draw a caret after the fraction
      ? self.Position.Plus(new PointF(self.DisplayBounds().Right, 0))
      // draw a caret before the fraction
      : self.Position;

    public static void HighlightCharacterAt<TFont, TGlyph>(
      this FractionDisplay<TFont, TGlyph> self,
      MathListIndex index, Color color) where TFont : IFont<TGlyph> {
      if (index.SubIndexInfo is { })
        throw new ArgumentException
          ("The subindex must be none to get the highlight a character in it.", nameof(index));
      self.Highlight(color);
    }

    public static void Highlight<TFont, TGlyph>(
      this FractionDisplay<TFont, TGlyph> self, Color color) where TFont : IFont<TGlyph> {
      self.Numerator.Highlight(color);
      self.Denominator.Highlight(color);
    }
  }
}