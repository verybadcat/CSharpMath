using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Editor {
  using Range = Atoms.Range;
  using Color = Structures.Color;
  using CSharpMath.Display.Text;
  using Display;
  using FrontEnd;
  public static partial class DisplayEditingExtensions {
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static ArgumentOutOfRangeException Arg(string name, string msg) =>
      throw new ArgumentException(msg, name);
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static ArgumentOutOfRangeException ArgOutOfRange(string name, object value, string msg) =>
      throw new ArgumentOutOfRangeException(name, value, msg);
    public static int CountCodepointsInRange(string str, Range range) {
      if (range.Location < 0)
        throw ArgOutOfRange(nameof(range), range, "The range starts from the negatives.");
      if (range.End >= str.Length)
        throw ArgOutOfRange(nameof(range), range, "The range extends beyond the end of the string.");
      int count = 0;
      for (int i = range.Location; i < range.End; i++, count++)
        if (char.IsSurrogate(str[i])) i++;
      return count;
    }

    public static int CodepointIndexToStringIndex(StringBuilder str, int codepointIndex) {
      if (codepointIndex < 0)
        throw ArgOutOfRange(nameof(codepointIndex), codepointIndex, "The codepoint index is negative.");
      for (int i = 0, count = 0; i < str.Length; i++, count++)
        if (count == codepointIndex) return i;
        else if (char.IsSurrogate(str[i])) i++;
      throw ArgOutOfRange(nameof(codepointIndex), codepointIndex, "The codepoint index is beyond the last codepoint of the string.");
    }

    public static int StringIndexToCodepointIndex(StringBuilder str, int stringIndex) {
      if (stringIndex < 0)
        throw ArgOutOfRange(nameof(stringIndex), stringIndex, "The string index is negative.");
      for (int i = 0, count = 0; i < str.Length; i++, count++) {
        if (char.IsSurrogate(str[i])) i++;
        if (i >= stringIndex) return count;
      }
      throw ArgOutOfRange(nameof(stringIndex), stringIndex, "The string index is beyond the last codepoint of the string.");
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
      foreach (var (bounds, kernAfter) in rects.Zip(line.KernedGlyphs.Select(g => g.KernAfterGlyph), ValueTuple.Create))
        if (bounds.Plus(new PointF(x, 0)).Contains(point))
          return i;
        else {
          x += bounds.Width + kernAfter;
          i++;
        }
      return null;
    }

    public static float XOffsetForStringIndex<TFont, TGlyph>(this AttributedGlyphRun<TFont, TGlyph> line, TypesettingContext<TFont, TGlyph> context, int index) where TFont : IFont<TGlyph> {
      if (index < 0)
        throw ArgOutOfRange(nameof(index), index, "The index is negative.");
      int i = 0;
      float x = 0;
      var rects = context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(line.Font, line.Glyphs, line.Length);
      foreach (var (bounds, kernAfter) in rects.Zip(line.KernedGlyphs.Select(g => g.KernAfterGlyph), ValueTuple.Create))
        if (i++ >= index)
          return x;
        else
          x += bounds.Width + kernAfter;
      throw ArgOutOfRange(nameof(index), index, "The index is beyond the end of the string.");
    }

#warning !!Ported incorrectly? https://github.com/kostub/MathEditor/blob/master/mathEditor/internal/MTDisplay%2BEditing.m#L184
    // Convert the index into the current string to an index into the mathlist. These may not be the same since a single
    // math atom may have multiple characters.
    public static int StringIndexToMathListIndex<TFont, TGlyph>(this TextLineDisplay<TFont, TGlyph> self, int strIndex) where TFont : IFont<TGlyph> {
      int strLenCovered = 0;
      for (int mlIndex = 0; mlIndex < self.Atoms.Length; mlIndex++) {
        if (strLenCovered >= strIndex)
          return mlIndex;
        strLenCovered += self.Atoms[mlIndex].Nucleus.Length;
      }
      if (strLenCovered < strIndex)
        throw new InvalidCodePathException("StrIndex should not be more than the len covered");
      return self.Atoms.Length;
    }
    public static int MathListIndexToStringIndex<TFont, TGlyph>(this TextLineDisplay<TFont, TGlyph> self, int mlIndex) where TFont : IFont<TGlyph> {
      if (mlIndex >= self.Atoms.Length)
        throw ArgOutOfRange(nameof(mlIndex), mlIndex, $"Index {mlIndex} not in range {self.Atoms.Length}");
      int strIndex = 0;
      for (int i = 0; i < mlIndex; i++) {
        strIndex += self.Atoms[i].Nucleus.Length;
      }
      return strIndex;
    }

    [NullableReference]
    public static MathListIndex IndexForPoint<TFont, TGlyph>(this TextLineDisplay<TFont, TGlyph> self, TypesettingContext<TFont, TGlyph> context, PointF point) where TFont : IFont<TGlyph> {
      // Convert the point to the reference of the CTLine
      var relativePoint = new PointF(point.X - self.Position.X, point.Y - self.Position.Y);
      var index = self.Runs.Select(run => run.Run.StringIndexForPosition(context, point.Plus(run.Position))).Single(x => x.HasValue).GetValueOrDefault();
      // The index returned is in UTF-16, translate to codepoint index.
      // NSUInteger codePointIndex = stringIndexToCodePointIndex(self.attributedString.string, index);
      // Convert the code point index to an index into the mathlist
      var mlIndex = self.StringIndexToMathListIndex(index);
      // index will be between 0 and _range.length inclusive
      // translate to the current index
      return MathListIndex.Level0Index(self.Range.Location + mlIndex);
    }

    public static PointF? PointForIndex<TFont, TGlyph>(this TextLineDisplay<TFont, TGlyph> self, TypesettingContext<TFont, TGlyph> context, MathListIndex index) where TFont : IFont<TGlyph> {
      float offset;
      if (!(index.SubIndexType is MathListSubIndexType.None))
        throw Arg(nameof(index), $"An index in a {nameof(TextLineDisplay<TFont, TGlyph>)} cannot have sub-indexes.");
      if (index.AtomIndex == self.Range.End)
        offset = self.Width;
      else {
        if (!self.Range.Contains(index.AtomIndex))
          throw ArgOutOfRange(nameof(index), index, $"The index {index} is not in the range {self.Range}.");
        var strIndex = self.MathListIndexToStringIndex(index.AtomIndex - self.Range.Location);
        int i = 0;
        offset = self.Runs.First(run => run.)XOffsetForStringIndex(context, strIndex);
      }
      return self.Position.Plus(new PointF(offset, 0));
    }

    public static void HighlightCharacterAtIndex<TFont, TGlyph>(this TextRunDisplay<TFont, TGlyph> self, int glyphIndex, Color color) where TFont : IFont<TGlyph> {
      if (!self.Range.Contains(glyphIndex))
        throw ArgOutOfRange(nameof(glyphIndex), glyphIndex, $"The index {glyphIndex} is not in the range {self.Range}.");
      // index is in unicode code points, while attrString is not
      var charIndex = CodepointIndexToStringIndex(self.Run.Text, glyphIndex - self.Range.Location);
      self.Run.Foreground = color;
    }
  }
}
