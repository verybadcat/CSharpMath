namespace CSharpMath.Editor {
  using System;
  using System.Drawing;
  using System.Linq;

  using Display;
  using Display.Text;
  using FrontEnd;
  using Color = Structures.Color;

  partial class DisplayEditingExtensions {
    public static MathListIndex IndexForPoint<TFont, TGlyph>(this FractionDisplay<TFont, TGlyph> self, TypesettingContext<TFont, TGlyph> context, PointF point) where TFont : IFont<TGlyph> {
      // We can be before or after the fraction
      if (point.X < self.Position.X - PixelDelta)
        //We are before the fraction, so
        return MathListIndex.Level0Index(self.Range.Location);
      else if (point.X > self.Position.X + self.Width + PixelDelta)
        //We are after the fraction
        return MathListIndex.Level0Index(self.Range.End);

      //We can be either near the numerator or denominator
      var numeratorDistance = DistanceBetweenY(point, self.Numerator.Position);
      var denominatorDistance = DistanceBetweenY(point, self.Denominator.Position);
      if (numeratorDistance < denominatorDistance)
        return MathListIndex.IndexAtLocation(self.Range.Location, self.Numerator.IndexForPoint(context, point), MathListSubIndexType.Numerator);
      else
        return MathListIndex.IndexAtLocation(self.Range.Location, self.Denominator.IndexForPoint(context, point), MathListSubIndexType.Denominator);
    }
    
    public static PointF? PointForIndex<TFont, TGlyph>(this FractionDisplay<TFont, TGlyph> self, TypesettingContext<TFont, TGlyph> context, MathListIndex index) where TFont : IFont<TGlyph> {
      if (index.SubIndexType != MathListSubIndexType.None)
        throw Arg("The subindex must be none to get the closest point for it.", nameof(index));
      if (index.AtomIndex == self.Range.End)
        // draw a caret after the fraction
        return new PointF(self.DisplayBounds.Right, self.Position.Y);
      // draw a caret before the fraction
      return new PointF(self.DisplayBounds.Left, self.Position.Y);
    }

    public static void HighlightCharacterAt<TFont, TGlyph>(this FractionDisplay<TFont, TGlyph> self, MathListIndex index, Color color) where TFont : IFont<TGlyph> {
      if (index.SubIndexType != MathListSubIndexType.None)
        throw Arg("The subindex must be none to get the highlight a character in it.", nameof(index));
      self.Highlight(color);
    }

    public static void Highlight<TFont, TGlyph>(this FractionDisplay<TFont, TGlyph> self, Color color) where TFont : IFont<TGlyph> {
      self.Numerator.Highlight(color);
      self.Denominator.Highlight(color);
    }

    public static ListDisplay<TFont, TGlyph> SubListForIndexType<TFont, TGlyph>(this FractionDisplay<TFont, TGlyph> self, MathListSubIndexType type) where TFont : IFont<TGlyph> {
      switch (type) {
        case MathListSubIndexType.Numerator:
          return self.Numerator;
        case MathListSubIndexType.Denominator:
          return self.Denominator;
        default:
          throw ArgOutOfRange("Subindex type is not a fraction subtype.", type, nameof(type));
      }
    }
  }
}
