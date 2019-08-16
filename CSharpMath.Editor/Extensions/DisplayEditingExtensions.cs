using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Editor {
  using Range = Atoms.Range;
  using Display;

  public static partial class DisplayEditingExtensions {
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static ArgumentException Arg(string msg, string name) => new ArgumentException(msg, name);
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static ArgumentOutOfRangeException ArgOutOfRange(string msg, object value, string name) => new ArgumentOutOfRangeException(name, value, msg);

    ///<summary>Number of pixels outside the bound to allow a point to be considered as part of the bounds.</summary>
    public const float PixelDelta = 2;

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
    public static float DistanceBetweenY(PointF p1, PointF p2) => (p1.Y - p2.Y) * (p1.Y - p2.Y);
  
    /// <summary>
    /// Finds the index in the mathlist before which a new character should be inserted.Returns null if it cannot find the index.
    /// </summary>
    /// <returns>Null if it cannot find the index.</returns>
    [NullableReference]
    public static MathListIndex IndexForPoint<TFont, TGlyph>(this IDisplay<TFont, TGlyph> display, FrontEnd.TypesettingContext<TFont, TGlyph> context, PointF point) where TFont : IFont<TGlyph> {
      switch (display) {
        case TextLineDisplay<TFont, TGlyph> text:
          return text.IndexForPoint(context, point);
        case FractionDisplay<TFont, TGlyph> frac:
          return frac.IndexForPoint(context, point);
        case RadicalDisplay<TFont, TGlyph> radical:
          return radical.IndexForPoint(context, point);
        case ListDisplay<TFont, TGlyph> list:
          return list.IndexForPoint(context, point);
        default:
          return null;
      }
    }
    ///<summary>The bounds of the display indicated by the given index</summary>
    public static PointF? PointForIndex<TFont, TGlyph>(this IDisplay<TFont, TGlyph> display, FrontEnd.TypesettingContext<TFont, TGlyph> context, MathListIndex index) where TFont : IFont<TGlyph> {
      switch (display) {
        case TextLineDisplay<TFont, TGlyph> text:
          return text.PointForIndex(context, index);
        case FractionDisplay<TFont, TGlyph> frac:
          return frac.PointForIndex(context, index);
        case RadicalDisplay<TFont, TGlyph> radical:
          return radical.PointForIndex(context, index);
        case ListDisplay<TFont, TGlyph> list:
          return list.PointForIndex(context, index);
        default:
          return null;
      }
    }
    public static void HighlightCharacterAt<TFont, TGlyph>(this IDisplay<TFont, TGlyph> display, MathListIndex index, Structures.Color color) where TFont : IFont<TGlyph> {
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
        default:
          break;
      }
    }
    public static void Highlight<TFont, TGlyph>(this IDisplay<TFont, TGlyph> display, Structures.Color color) where TFont : IFont<TGlyph> {
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
        default:
          break;
      }
    }
  }
}