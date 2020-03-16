using System.Drawing;

namespace CSharpMath.Editor {
  using Display;
  using Display.Displays;
  using Display.FrontEnd;
  public static partial class EditingExtensions {
    /// <summary>
    /// Number of pixels outside the bound to allow a point to be considered as part of the bounds.
    /// </summary>
    public const float PixelDelta = 2;

    /// <summary>
    /// Calculates the manhattan distance from a point to the nearest boundary of the rectangle.
    /// </summary>
    public static float DistanceFromPointToRect(PointF point, RectangleF rect) {
      float distance = 0;
      if (point.X < rect.X)
        distance += rect.X - point.X;
      else if (point.X > rect.Right)
        distance += point.X - rect.Right;

      if (point.Y < rect.Y)
        distance += rect.Y - point.Y;
      else if (point.Y > rect.YMax())
        distance += point.Y - rect.YMax();
      return distance;
    }

    /// <summary>
    /// Finds the index in the mathlist before which a new character should be inserted.
    /// Returns null if it cannot find the index.
    /// </summary>
    public static MathListIndex? IndexForPoint<TFont, TGlyph>(
      this IDisplay<TFont, TGlyph> display,
      TypesettingContext<TFont, TGlyph> context, PointF point)
      where TFont : IFont<TGlyph> => display switch
    {
      TextLineDisplay<TFont, TGlyph> text => text.IndexForPoint(context, point),
      FractionDisplay<TFont, TGlyph> frac => frac.IndexForPoint(context, point),
      RadicalDisplay<TFont, TGlyph> radical => radical.IndexForPoint(context, point),
      ListDisplay<TFont, TGlyph> list => list.IndexForPoint(context, point),
      LargeOpLimitsDisplay<TFont, TGlyph> largeOp => largeOp.IndexForPoint(context, point),
      IGlyphDisplay<TFont, TGlyph> glyph => glyph.IndexForPoint(context, point),
      _ => null,
    };
    ///<summary>The bounds of the display indicated by the given index</summary>
    public static PointF? PointForIndex<TFont, TGlyph>(this IDisplay<TFont, TGlyph> display,
      TypesettingContext<TFont, TGlyph> context,
      MathListIndex index) where TFont : IFont<TGlyph> => display switch
    {
      TextLineDisplay<TFont, TGlyph> text => text.PointForIndex(context, index),
      FractionDisplay<TFont, TGlyph> frac => frac.PointForIndex(context, index),
      RadicalDisplay<TFont, TGlyph> radical => radical.PointForIndex(context, index),
      ListDisplay<TFont, TGlyph> list => list.PointForIndex(context, index),
      LargeOpLimitsDisplay<TFont, TGlyph> largeOp => largeOp.PointForIndex(context, index),
      IGlyphDisplay<TFont, TGlyph> glyph => glyph.PointForIndex(context, index),
      _ => null,
    };
    public static void HighlightCharacterAt<TFont, TGlyph>
      (this IDisplay<TFont, TGlyph> display, MathListIndex index, Structures.Color color)
      where TFont : IFont<TGlyph> {
      switch (display) {
        case TextLineDisplay<TFont, TGlyph> text:
          text.HighlightCharacterAt(index, color);
          break;
        case FractionDisplay<TFont, TGlyph> frac:
          frac.HighlightCharacterAt(index, color);
          break;
        case RadicalDisplay<TFont, TGlyph> radical:
          radical.HighlightCharacterAt(index, color);
          break;
        case ListDisplay<TFont, TGlyph> list:
          list.HighlightCharacterAt(index, color);
          break;
        case LargeOpLimitsDisplay<TFont, TGlyph> largeOp:
          largeOp.HighlightCharacterAt(index, color);
          break;
        case IGlyphDisplay<TFont, TGlyph> glyph:
          glyph.HighlightCharacterAt(index, color);
          break;
        default:
          break;
      }
    }
    public static void Highlight<TFont, TGlyph>(this IDisplay<TFont, TGlyph> display, Structures.Color color)
      where TFont : IFont<TGlyph> {
      switch (display) {
        case TextLineDisplay<TFont, TGlyph> text:
          text.Highlight(color);
          break;
        case FractionDisplay<TFont, TGlyph> frac:
          frac.Highlight(color);
          break;
        case RadicalDisplay<TFont, TGlyph> radical:
          radical.Highlight(color);
          break;
        case ListDisplay<TFont, TGlyph> list:
          list.Highlight(color);
          break;
        case LargeOpLimitsDisplay<TFont, TGlyph> largeOp:
          largeOp.Highlight(color);
          break;
        case IGlyphDisplay<TFont, TGlyph> glyph:
          glyph.Highlight(color);
          break;
        default:
          break;
      }
    }
  }
}