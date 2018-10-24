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

    /// <summary>
    /// Finds the index in the mathlist before which a new character should be inserted.
    /// </summary>
    /// <returns>Null if it cannot find the index.</returns>
    [NullableReference]
    public static MathListIndex ClosestIndexToPoint<TFont, TGlyph>(this IDisplay<TFont, TGlyph> display, PointF point) where TFont : Display.IFont<TGlyph> {
      switch (display) {
        case 
      }
    }
  }
}
