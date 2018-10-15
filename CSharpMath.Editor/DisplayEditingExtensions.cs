using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Editor {
  using Atoms;
  using Display;
  using FrontEnd;
  public static partial class DisplayEditingExtensions {
    public static int CountCodepointsInRange(string str, Range range) {
      if (range.Location < 0)
        throw new ArgumentOutOfRangeException("The range starts from the negatives.");
      if (range.End >= str.Length)
        throw new ArgumentOutOfRangeException("The range extends beyond the end of the string.");
      int count = 0;
      for (int i = range.Location; i < range.End; i++, count++)
        if (char.IsSurrogate(str[i])) i++;
      return count;
    }

    public static int CodepointIndexToStringIndex(string str, int codepointIndex) {
      if (codepointIndex < 0)
        throw new ArgumentOutOfRangeException("The codepoint index is negative.");
      for (int i = 0, count = 0; i < str.Length; i++, count++)
        if (count == codepointIndex) return i;
        else if (char.IsSurrogate(str[i])) i++;
      throw new ArgumentOutOfRangeException("The codepoint index is beyond the last codepoint of the string.");
    }

    ///<summary>Calculates the manhattan distance from a point to the nearest boundary of the rectangle.</summary>
    public static float DistanceFromPointToRect(PointF point, RectangleF rect) {
      float distance = 0;
      if (point.X < rect.X) {
        distance += rect.X - point.X;
      } else if (point.X > rect.Right) {
        distance += point.X - rect.Right;
      }

      if (point.Y < rect.Y) {
        distance += (rect.Y - point.Y);
      } else if (point.Y > rect.YMax()) {
        distance += point.Y - rect.YMax();
      }
      return distance;
    }

    public static MathListIndex ClosestIndexToParent<TFont, TGlyph>(this TextRunDisplay<TFont, TGlyph> self, TypesettingContext<TFont, TGlyph> context, PointF point) where TFont : IFont<TGlyph> {
      // Convert the point to the reference of the CTLine
      var relativePoint = new PointF(point.X - self.Position.X, point.Y - self.Position.Y);
      context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(self.Run.Font, self.Run.gl)
      context.GlyphBoundsProvider.GetBoundingRectsForGlyphs
    }
  }
}
