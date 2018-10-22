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
    static ArgumentException Arg(string msg, string name) => new ArgumentException(msg, name);
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static ArgumentOutOfRangeException ArgOutOfRange(string msg, object value, string name) => new ArgumentOutOfRangeException(name, value, msg);
    public static int CountCodepoints(StringBuilder str) {
      int count = 0;
      for (int i = 0; i < str.Length; i++, count++)
        if (char.IsSurrogate(str[i])) i++;
      return count;
    }

    public static int CountCodepointsInRange(StringBuilder str, Range range) {
      if (range.Location < 0)
        throw ArgOutOfRange("The range starts from the negatives.", range, nameof(range));
      if (range.End >= str.Length)
        throw ArgOutOfRange("The range extends beyond the end of the string.", range, nameof(range));
      int count = 0;
      for (int i = range.Location; i < range.End; i++, count++)
        if (char.IsSurrogate(str[i])) i++;
      return count;
    }

    public static int CodepointIndexToStringIndex(StringBuilder str, int codepointIndex) {
      if (codepointIndex < 0)
        throw ArgOutOfRange("The codepoint index is negative.", codepointIndex, nameof(codepointIndex));
      for (int i = 0, count = 0; i < str.Length; i++, count++)
        if (count == codepointIndex) return i;
        else if (char.IsSurrogate(str[i])) i++;
      throw ArgOutOfRange("The codepoint index is beyond the last codepoint of the string.", codepointIndex, nameof(codepointIndex));
    }

    public static int StringIndexToCodepointIndex(StringBuilder str, int stringIndex) {
      if (stringIndex < 0)
        throw ArgOutOfRange("The string index is negative.", stringIndex, nameof(stringIndex));
      for (int i = 0, count = 0; i < str.Length; i++, count++) {
        if (char.IsSurrogate(str[i])) i++;
        if (i >= stringIndex) return count;
      }
      throw ArgOutOfRange("The string index is beyond the last codepoint of the string.", stringIndex, nameof(stringIndex));
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
      var rects = context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(line.Font, line.Glyphs.AsForEach(), line.Length);
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
        throw ArgOutOfRange("The index is negative.", index, nameof(index));
      int i = 0;
      float x = 0;
      var rects = context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(line.Font, line.Glyphs.AsForEach(), line.Length);
      foreach (var (bounds, kernAfter) in rects.Zip(line.KernedGlyphs.Select(g => g.KernAfterGlyph), ValueTuple.Create))
        if (i++ >= index)
          return x;
        else
          x += bounds.Width + kernAfter;
      throw ArgOutOfRange("The index is beyond the end of the string.", index, nameof(index));
    }
    
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
        throw ArgOutOfRange($"The index is not in the range {self.Atoms.Length}", mlIndex, nameof(mlIndex));
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

    public static (TextRunDisplay<TFont, TGlyph> run, int charIndex) GetRunAndCharIndexFromStringIndex<TFont, TGlyph>(this TextLineDisplay<TFont, TGlyph> self, int lineCharIndex) where TFont : IFont<TGlyph> {
      var currentRun = self.Runs.First(s => (lineCharIndex -= s.Run.Text.Length) < 0);
      return (currentRun, currentRun.Run.Text.Length + lineCharIndex); //offset for target char in its containing string
      /* //Quick test in C# Interactive
int strIndex = 6; //offset for target char if all strings in array are fused together
var ss = new[] { "abcde", "fgh", "i", "f", "g" };
var c = ss.First(s => (strIndex -= s.Length) < 0);
return c.Length + strIndex; //offset for target char in its containing string
*/
    }

    public static (TextRunDisplay<TFont, TGlyph> run, int charIndex) GetRunAndCharIndexFromCodepointIndex<TFont, TGlyph>(this TextLineDisplay<TFont, TGlyph> self, int lineCharIndex) where TFont : IFont<TGlyph> {
      var currentRun = self.Runs.First(s => (lineCharIndex -= CountCodepoints(s.Run.Text)) < 0);
      return (currentRun, currentRun.Run.Length + lineCharIndex); //offset for target char in its containing string
    }

    public static PointF? PointForIndex<TFont, TGlyph>(this TextLineDisplay<TFont, TGlyph> self, TypesettingContext<TFont, TGlyph> context, MathListIndex index) where TFont : IFont<TGlyph> {
      float offset;
      if (!(index.SubIndexType is MathListSubIndexType.None))
        throw Arg($"An index in a {nameof(TextLineDisplay<TFont, TGlyph>)} cannot have sub-indexes.", nameof(index));
      if (index.AtomIndex == self.Range.End)
        offset = self.Width;
      else {
        if (!self.Range.Contains(index.AtomIndex))
          throw ArgOutOfRange($"The index is not in the range {self.Range}.", index, nameof(index));
        var strIndex = self.MathListIndexToStringIndex(index.AtomIndex - self.Range.Location);
        var (run, charIndex) = self.GetRunAndCharIndexFromStringIndex(strIndex);
        offset = run.Position.X + run.Run.XOffsetForStringIndex(context, charIndex);
      }
      return self.Position.Plus(new PointF(offset, 0));
    }

    public static void HighlightCharacterAt<TFont, TGlyph>(this TextLineDisplay<TFont, TGlyph> self, MathListIndex index, Color color) where TFont : IFont<TGlyph> {
      if (!self.Range.Contains(index.AtomIndex))
        throw ArgOutOfRange($"The index is not in the range {self.Range}.", index, nameof(index));
      if (index.SubIndexType is MathListSubIndexType.None)
        throw Arg("The subindex type must not be none to be able to highlight it.", nameof(index));
      if (index.SubIndexType is MathListSubIndexType.Nucleus)
        throw Arg("Nucleus highlighting is not supported.", nameof(index));
      // index is in unicode code points, while attrString is not
      var (run, charIndex) = self.GetRunAndCharIndexFromCodepointIndex(index.AtomIndex - self.Range.Location);
      var runIndex = self.Runs.IndexOf(run);
      if (runIndex is -1) throw new InvalidCodePathException("run must be in self.Runs.");
      self.Runs[runIndex] = new TextRunDisplay<TFont, TGlyph>(run, run.Range.Slice(0, ))
      run.Run.Foreground = color;
    }
  }
}
