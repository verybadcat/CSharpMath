namespace CSharpMath.Editor {
  using System;
  using System.Drawing;

  using Displays;
  using Displays.Display;
  using FrontEnd;
  using Color = Structures.Color;

  partial class DisplayEditingExtensions {
    public static MathListIndex IndexForPoint<TFont, TGlyph>(
      this FractionDisplay<TFont, TGlyph> self,
      TypesettingContext<TFont, TGlyph> context,
      PointF point) where TFont : IFont<TGlyph> =>
      // We can be before or after the fraction
      point.X < self.Position.X - PixelDelta
      //We are before the fraction, so
      ? MathListIndex.Level0Index(self.Range.Location)
      : point.X > self.Position.X + self.Width + PixelDelta
      //We are after the fraction
      ? MathListIndex.Level0Index(self.Range.End)
      : point.Y > self.LinePosition + PixelDelta
      ? MathListIndex.IndexAtLocation(self.Range.Location,
          MathListSubIndexType.Numerator, self.Numerator.IndexForPoint(context, point))
      : point.Y < self.LinePosition - PixelDelta
      ? MathListIndex.IndexAtLocation(self.Range.Location,
          MathListSubIndexType.Denominator, self.Denominator.IndexForPoint(context, point))
      : point.X > self.Position.X + self.Width / 2
      ? MathListIndex.Level0Index(self.Range.End)
      : MathListIndex.Level0Index(self.Range.Location);

    public static PointF? PointForIndex<TFont, TGlyph>(
      this FractionDisplay<TFont, TGlyph> self,
      TypesettingContext<TFont, TGlyph> _,
      MathListIndex index) where TFont : IFont<TGlyph> =>
      index.SubIndexType != MathListSubIndexType.None
      ? throw new ArgumentException
        ("The subindex must be none to get the closest point for it.", nameof(index))
      : index.AtomIndex == self.Range.End
      // draw a caret after the fraction
      ? self.Position.Plus(new PointF(self.DisplayBounds.Right, 0))
      // draw a caret before the fraction
      : self.Position;

    public static void HighlightCharacterAt<TFont, TGlyph>(
      this FractionDisplay<TFont, TGlyph> self,
      MathListIndex index, Color color) where TFont : IFont<TGlyph> {
      if (index.SubIndexType != MathListSubIndexType.None)
        throw new ArgumentException
          ("The subindex must be none to get the highlight a character in it.", nameof(index));
      self.Highlight(color);
    }

    public static void Highlight<TFont, TGlyph>(
      this FractionDisplay<TFont, TGlyph> self, Color color) where TFont : IFont<TGlyph> {
      self.Numerator.Highlight(color);
      self.Denominator.Highlight(color);
    }

    public static ListDisplay<TFont, TGlyph> SubListForIndexType<TFont, TGlyph>(
      this FractionDisplay<TFont, TGlyph> self, MathListSubIndexType type) where TFont : IFont<TGlyph> =>
      type switch
      {
        MathListSubIndexType.Numerator => self.Numerator,
        MathListSubIndexType.Denominator => self.Denominator,
        _ => throw new ArgumentOutOfRangeException
          (nameof(type), type, "Subindex type is not a fraction subtype."),
      };
  }
}
