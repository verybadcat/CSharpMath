using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Editor {
  using Atoms;
  using CSharpMath.Display.Text;
  using Display;
  using FrontEnd;
  public static partial class DisplayEditingExtensions {
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static int ThrowArgOutOfRange(string name, string msg) =>
      throw new ArgumentOutOfRangeException(name, msg);
    public static int CountCodepointsInRange(string str, Range range) {
      if (range.Location < 0)
        ThrowArgOutOfRange(nameof(range), "The range starts from the negatives.");
      if (range.End >= str.Length)
        ThrowArgOutOfRange(nameof(range), "The range extends beyond the end of the string.");
      int count = 0;
      for (int i = range.Location; i < range.End; i++, count++)
        if (char.IsSurrogate(str[i])) i++;
      return count;
    }

    public static int CodepointIndexToStringIndex(string str, int codepointIndex) {
      if (codepointIndex < 0)
        ThrowArgOutOfRange(nameof(codepointIndex), "The codepoint index is negative.");
      for (int i = 0, count = 0; i < str.Length; i++, count++)
        if (count == codepointIndex) return i;
        else if (char.IsSurrogate(str[i])) i++;
      return ThrowArgOutOfRange(nameof(codepointIndex), "The codepoint index is beyond the last codepoint of the string.");
    }

    public static int StringIndexToCodepointIndex(StringBuilder str, int stringIndex) {
      if (stringIndex < 0)
        ThrowArgOutOfRange(nameof(stringIndex), "The string index is negative.");
      for (int i = 0, count = 0; i < str.Length; i++, count++) {
        if (char.IsSurrogate(str[i])) i++;
        if (i >= stringIndex) return count;
      }
      return ThrowArgOutOfRange(nameof(stringIndex), "The string index is beyond the last codepoint of the string.");
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

    public static int? StringIndexForPosition<TFont, TGlyph>(this AttributedGlyphRun<TFont, TGlyph> line, TypesettingContext<TFont, TGlyph> context, PointF point) where TFont : IFont<TGlyph> {
      int i = 0;
      float x = 0;
      var rects = context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(line.Font, line.Glyphs, line.Length);
      foreach(var (bounds, kernAfter) in rects.Zip(line.KernedGlyphs.Select(g => g.KernAfterGlyph), ValueTuple.Create))
        if (bounds.Plus(new PointF(x, 0)).Contains(point))
          return i;
        else
          x += bounds.Width + kernAfter;
      return null;
    }

#warning !!Ported incorrectly? https://github.com/kostub/MathEditor/blob/master/mathEditor/internal/MTDisplay%2BEditing.m#L184
    // Convert the index into the current string to an index into the mathlist. These may not be the same since a single
    // math atom may have multiple characters.
    public static int ConvertToGlyphIndex<TFont, TGlyph>(this TextRunDisplay<TFont, TGlyph> self, int strIndex) where TFont : IFont<TGlyph> =>
      StringIndexToCodepointIndex(self.Run.Text, strIndex);
    [NullableReference]
    public static MathListIndex IndexForPoint<TFont, TGlyph>(this TextRunDisplay<TFont, TGlyph> self, TypesettingContext<TFont, TGlyph> context, PointF point) where TFont : IFont<TGlyph> {
      // Convert the point to the reference of the CTLine
      var relativePoint = new PointF(point.X - self.Position.X, point.Y - self.Position.Y);
      if (!(self.Run.StringIndexForPosition(context, point) is int index))
        return null;
      // The index returned is in UTF-16, translate to codepoint index.
      // NSUInteger codePointIndex = stringIndexToCodePointIndex(self.attributedString.string, index);
      // Convert the code point index to an index into the mathlist
      var mlIndex = self.ConvertToGlyphIndex(index);
      // index will be between 0 and _range.length inclusive
      // translate to the current index
      return MathListIndex.Level0Index(self.Range.Location + mlIndex);
    }
  }
}
