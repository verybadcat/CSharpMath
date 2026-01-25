namespace CSharpMath.Editor {
  using System;
  using System.Drawing;
  using Display;
  using Display.Displays;
  using Display.FrontEnd;

  partial class Extensions {
    public static MathListIndex IndexForPoint<TFont, TGlyph>(
      this LargeOpLimitsDisplay<TFont, TGlyph> self,
      TypesettingContext<TFont, TGlyph> context,
      PointF point) where TFont : IFont<TGlyph> =>
      // We can be before or after the large operator
      point.X < self.Position.X - PixelDelta
      // We are before the large operator, so
      ? MathListIndex.Level0Index(self.Range.Location)
      : point.X > self.Position.X + self.Width + PixelDelta
      // We are after the large operator
      ? MathListIndex.Level0Index(self.Range.End)
      : self.UpperLimit is { } u && point.Y > self.Position.Y + u.Position.Y - PixelDelta
      ? MathListIndex.IndexAtLocation(self.Range.Location,
          MathListSubIndexType.Superscript, u.IndexForPoint(context, point))
      : self.LowerLimit is { } l && point.Y < self.Position.Y + l.Position.Y + l.DisplayBounds().Height + PixelDelta
      ? MathListIndex.IndexAtLocation(self.Range.Location,
          MathListSubIndexType.Subscript, l.IndexForPoint(context, point))
      : point.X > self.Position.X + self.Width * 3 / 4
      ? MathListIndex.Level0Index(self.Range.End)
      : point.X > self.Position.X + self.Width / 2
      ? MathListIndex.IndexAtLocation(self.Range.Location,
        MathListSubIndexType.BetweenBaseAndScripts, MathListIndex.Level0Index(1))
      : MathListIndex.Level0Index(self.Range.Location);

    public static PointF? PointForIndex<TFont, TGlyph>(
      this LargeOpLimitsDisplay<TFont, TGlyph> self,
      TypesettingContext<TFont, TGlyph> _,
      MathListIndex index) where TFont : IFont<TGlyph> =>
      index.SubIndexType != MathListSubIndexType.None
      ? throw new ArgumentException
        ("The subindex must be none to get the closest point for it.", nameof(index))
      : index.AtomIndex == self.Range.End
      // draw a caret after the nucleus for BetweenBaseAndScripts
      ? new PointF(self.NucleusDisplay.Frame().Right, 0)
      // draw a caret before the large operator
      : self.Position;

    public static void HighlightCharacterAt<TFont, TGlyph>(
      this LargeOpLimitsDisplay<TFont, TGlyph> self,
      MathListIndex index, Color color) where TFont : IFont<TGlyph> {
      if (index.SubIndexType != MathListSubIndexType.None)
        throw new ArgumentException
          ("The subindex must be none to get the highlight a character in it.", nameof(index));
      self.Highlight(color);
    }

    public static void Highlight<TFont, TGlyph>(
      this LargeOpLimitsDisplay<TFont, TGlyph> self, Color color) where TFont : IFont<TGlyph> {
      self.UpperLimit?.Highlight(color);
      self.LowerLimit?.Highlight(color);
    }

    public static IDisplay<TFont, TGlyph>? SubListForIndexType<TFont, TGlyph>(
      this LargeOpLimitsDisplay<TFont, TGlyph> self, MathListSubIndexType type) where TFont : IFont<TGlyph> =>
      type switch
      {
        MathListSubIndexType.Superscript => self.UpperLimit,
        MathListSubIndexType.Subscript => self.LowerLimit,
        _ => throw new ArgumentOutOfRangeException
          (nameof(type), type, "Subindex type is not a fraction subtype."),
      };
  }
}
