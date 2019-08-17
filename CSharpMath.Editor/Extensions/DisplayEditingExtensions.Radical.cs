namespace CSharpMath.Editor {
  using System;
  using System.Drawing;
  using System.Linq;

  using Display;
  using Display.Text;
  using FrontEnd;
  using Color = Structures.Color;

  partial class DisplayEditingExtensions {
    public static MathListIndex IndexForPoint<TFont, TGlyph>(this RadicalDisplay<TFont, TGlyph> self, TypesettingContext<TFont, TGlyph> context, PointF point) where TFont : IFont<TGlyph> {
      // We can be before or after the radical
      if (point.X < self.Position.X - PixelDelta)
        //We are before the radical, so
        return MathListIndex.Level0Index(self.Range.Location);
      else if (point.X > self.Position.X + self.Width + PixelDelta)
        //We are after the radical
        return MathListIndex.Level0Index(self.Range.End);

      //We can be either near the degree or the radicand
      var degreeRect = self.Degree != null ? new RectangleF(self.Degree.Position, self.Degree.DisplayBounds.Size) : default;
      var radicandRect = new RectangleF(self.Radicand.Position, self.Radicand.DisplayBounds.Size);
      var degreeDistance = DistanceFromPointToRect(point, degreeRect);
      var radicandDistance = DistanceFromPointToRect(point, radicandRect);
      if (degreeDistance < radicandDistance) {
        if (self.Degree != null)
          return MathListIndex.IndexAtLocation(self.Range.Location, self.Degree.IndexForPoint(context, point), MathListSubIndexType.Degree);
        return MathListIndex.Level0Index(self.Range.Location);
      } else
        return MathListIndex.IndexAtLocation(self.Range.Location, self.Radicand.IndexForPoint(context, point), MathListSubIndexType.Radicand);
    }
    
    public static PointF? PointForIndex<TFont, TGlyph>(this RadicalDisplay<TFont, TGlyph> self, TypesettingContext<TFont, TGlyph> context, MathListIndex index) where TFont : IFont<TGlyph> {
      if (index.SubIndexType != MathListSubIndexType.None)
        throw Arg("The subindex must be none to get the closest point for it.", nameof(index));

      if (index.AtomIndex == self.Range.End)
        // draw a caret after the radical
        return new PointF(self.DisplayBounds.Right, self.Position.Y);

      return new PointF(self.DisplayBounds.Left, self.Position.Y);
    }

    public static void HighlightCharacterAt<TFont, TGlyph>(this RadicalDisplay<TFont, TGlyph> self, MathListIndex index, Color color) where TFont : IFont<TGlyph> {
      if (index.SubIndexType != MathListSubIndexType.None)
        throw Arg("The subindex must be none to get the highlight a character in it.", nameof(index));
      self.Highlight(color);
    }

    public static void Highlight<TFont, TGlyph>(this RadicalDisplay<TFont, TGlyph> self, Color color) where TFont : IFont<TGlyph> {
#warning Is including Degree intended? It is not present in iosMath
      self.Degree.Highlight(color);
      self.Radicand.Highlight(color);
    }

    public static ListDisplay<TFont, TGlyph> SubListForIndexType<TFont, TGlyph>(this RadicalDisplay<TFont, TGlyph> self, MathListSubIndexType type) where TFont : IFont<TGlyph> {
      switch (type) {
        case MathListSubIndexType.Radicand:
          return self.Radicand;
        case MathListSubIndexType.Degree:
          return self.Degree;
        default:
          throw ArgOutOfRange("Subindex type is not a radical subtype.", type, nameof(type));
      }
    }
  }
}
