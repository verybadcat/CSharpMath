namespace CSharpMath.Editor {
  using System;
  using System.Drawing;
  using System.Linq;

  using Display;
  using Display.Text;
  using FrontEnd;
  using Color = Structures.Color;

  public partial class DisplayEditingExtensions {
    public static int? StringIndexForXOffset<TFont, TGlyph>(this AttributedGlyphRun<TFont, TGlyph> line, TypesettingContext<TFont, TGlyph> context, float offset) where TFont : IFont<TGlyph> {
      if (offset < 0) return 0; //Move cursor to index 0
      int i = 0;
      float x = 0;
      var advances = context.GlyphBoundsProvider.GetAdvancesForGlyphs(line.Font, line.Glyphs.AsForEach(), line.Length).Advances;
      foreach (var (advance, kernAfter) in advances.Zip(line.GlyphInfos.Select(g => g.KernAfterGlyph), ValueTuple.Create))
        if (x <= offset && offset < advance + x)
          return i;
        else {
          x += advance + kernAfter;
          i++;
          if (offset < x) //If the point is in the kern after this, then the index is the one after this
            return i;
        }
      return null;
    }

    public static float XOffsetForStringIndex<TFont, TGlyph>(this AttributedGlyphRun<TFont, TGlyph> line, TypesettingContext<TFont, TGlyph> context, int index) where TFont : IFont<TGlyph> {
      if (index < 0)
        throw ArgOutOfRange("The index is negative.", index, nameof(index));
      int i = 0;
      float x = 0;
      var advances = context.GlyphBoundsProvider.GetAdvancesForGlyphs(line.Font, line.Glyphs.AsForEach(), line.Length).Advances;
      foreach (var (advance, kernAfter) in advances.Zip(line.GlyphInfos.Select(g => g.KernAfterGlyph), ValueTuple.Create))
        if (i++ >= index)
          return x;
        else
          x += advance + kernAfter;
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

    public static MathListIndex IndexForPoint<TFont, TGlyph>(this TextLineDisplay<TFont, TGlyph> self, TypesettingContext<TFont, TGlyph> context, PointF point) where TFont : IFont<TGlyph> {
      // Convert the point to the reference of the CTLine
      var relativePoint = new PointF(point.X - self.Position.X, point.Y - self.Position.Y);
      var indices = self.Runs.Select(run => run.Run.StringIndexForXOffset(context, relativePoint.Plus(run.Position).X)).Where(x => x.HasValue);
      if (indices.IsEmpty())
        return null;
      var index = indices.Single().GetValueOrDefault();
      // The index returned is in UTF-16, translate to codepoint index.
      // NSUInteger codePointIndex = stringIndexToCodePointIndex(self.attributedString.string, index);
      // Convert the code point index to an index into the mathlist
      var mlIndex = self.StringIndexToMathListIndex(index);
      // index will be between 0 and _range.length inclusive
      if (mlIndex < 0 || mlIndex > self.Range.Length)
        throw new InvalidCodePathException($"Returned index out of range: {index}, range ({self.Range.Location}, {self.Range.Length})");
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
      run.Run.GlyphInfos[charIndex].Foreground = color;
    }

    public static void Highlight<TFont, TGlyph>(this TextLineDisplay<TFont, TGlyph> self, Color color) where TFont : IFont<TGlyph> {
      var iMax = self.Runs.Count;
      for (int i = 0; i < iMax; i++) {
        var run = self.Runs[i].Run;
        for (int j = 0; j < run.Length; j++)
          run.GlyphInfos[j].Foreground = color;
      }
    }
  }
}
