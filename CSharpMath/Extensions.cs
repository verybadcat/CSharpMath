using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CSharpMath {
  using Display;
  using Display.FrontEnd;
  public static partial class Extensions {
    public static string ToStringInvariant<T>(this T value) where T : IConvertible =>
      value.ToString(CultureInfo.InvariantCulture);
    public static string ToStringInvariant<T>(this T value, string? format = null)
      where T : IFormattable => value.ToString(format, CultureInfo.InvariantCulture);
    public static bool IsEmpty<T>(this IEnumerable<T> enumerable) {
      foreach (var _ in enumerable) return false;
      return true;
    }
    public static bool IsNonEmpty<T>(this IEnumerable<T> enumerable) => !enumerable.IsEmpty();
    public static IEnumerable<TResult> Zip<TFirst, TSecond, TThird, TResult>(
      this IEnumerable<TFirst> first, IEnumerable<TSecond> second, IEnumerable<TThird> third,
      Func<TFirst, TSecond, TThird, TResult> resultSelector) {
      return ZipIterator();
      IEnumerable<TResult> ZipIterator() {
        using var enum1 = first.GetEnumerator();
        using var enum2 = second.GetEnumerator();
        using var enum3 = third.GetEnumerator();
        while (enum1.MoveNext() && enum2.MoveNext() && enum3.MoveNext()) {
          yield return resultSelector(
              enum1.Current,
              enum2.Current,
              enum3.Current);
        }
      }
    }
    public static bool SequenceEqual<T>(this IEnumerable<T> enumerable,
      IEnumerable<T> otherEnumerable, Func<T, T, bool>? equalityTester = null) {
      if (equalityTester == null) equalityTester = EqualityComparer<T>.Default.Equals;
      bool r;
      if (enumerable == null && otherEnumerable == null) {
        r = true;
      } else if (enumerable != null && otherEnumerable != null) {
        if (r = enumerable.Count() == otherEnumerable.Count()) {
          using var enum1 = enumerable.GetEnumerator();
          using var enum2 = otherEnumerable.GetEnumerator();
          while (r && enum1.MoveNext() && enum2.MoveNext()) {
            r = equalityTester(enum1.Current, enum2.Current);
          }
        }
      } else {
        // one enumerable is null and the other isn't
        r = false;
      }
      return r;
    }
    public static float CollectionAscent<TFont, TGlyph>
      (this IEnumerable<IDisplay<TFont, TGlyph>> displays) where TFont : IFont<TGlyph> =>
        displays.IsNonEmpty() ? displays.Max(display => display.Ascent + display.Position.Y) : 0;
    public static float CollectionDescent<TFont, TGlyph>
      (this IEnumerable<IDisplay<TFont, TGlyph>> displays) where TFont : IFont<TGlyph> =>
        displays.IsNonEmpty() ? displays.Max(display => display.Descent - display.Position.Y) : 0;
    public static float CollectionWidth<TFont, TGlyph>
      (this IEnumerable<IDisplay<TFont, TGlyph>> displays) where TFont : IFont<TGlyph> =>
      displays.IsNonEmpty()
      ? displays.Max(d => d.Position.X + d.Width) - displays.Min(d => d.Position.X)
      : 0;
    public static PointF Plus(this PointF point1, PointF point2) =>
      new PointF(point1.X + point2.X, point1.Y + point2.Y);
    public static bool Is(this ReadOnlySpan<char> span, char c) =>
      span.Length == 1 && span[0] == c;
    public static bool IsNot(this ReadOnlySpan<char> span, char c) =>
      span.Length != 1 || span[0] != c;
    public static bool Is(this ReadOnlySpan<char> span, string s) =>
      span.SequenceEqual(s.AsSpan());
    public static bool IsNot(this ReadOnlySpan<char> span, string s) =>
      span.SequenceEqual(s.AsSpan());
    public static bool StartsWithInvariant(this ReadOnlySpan<char> str, string prefix) =>
      str.StartsWith(prefix.AsSpan(), StringComparison.OrdinalIgnoreCase);
    public static ReadOnlySpan<char> RemovePrefix(this ReadOnlySpan<char> str,
      string prefix, StringComparison compare = StringComparison.OrdinalIgnoreCase) =>
      str.StartsWith(prefix.AsSpan(), compare) ? str.Slice(prefix.Length) : str;
    public static RectangleF Plus(this RectangleF rect, PointF vector) =>
      new RectangleF(rect.Location.Plus(vector), rect.Size);
    public static RectangleF Union(this RectangleF rect1, RectangleF rect2) {
      var x = Math.Min(rect1.X, rect2.X);
      var y = Math.Min(rect1.Y, rect2.Y);
      var maxX = Math.Max(rect1.Right, rect2.Right);
      var maxY = Math.Max(rect1.Bottom, rect2.Bottom);
      return new RectangleF(x, y, maxX - x, maxY - y);
    }
    /// <summary>Because we are NOT inverting our y axis,
    /// the properties "Top" and "Bottom" have misleading names.</summary>
    public static float YMin(this RectangleF rect) => rect.Top;
    /// <summary>Because we are NOT inverting our y axis,
    /// the properties "Top" and "Bottom" have misleading names.</summary>
    public static float YMax(this RectangleF rect) => rect.Bottom;
    public static void GetAscentDescentWidth(this RectangleF rect,
      out float ascent, out float descent, out float width) {
      ascent = rect.Bottom;
      width = rect.Width;
      descent = -rect.Y;
    }
    [return: System.Diagnostics.CodeAnalysis.MaybeNull]
    public static T PeekOrDefault<T>(this Stack<T> stack) => stack.Count == 0 ? default : stack.Peek();
    public static StringBuilder Append(this StringBuilder sb, ReadOnlySpan<char> value) {
      sb.EnsureCapacity(sb.Length + value.Length);
      for (int i = 0; i < value.Length; i++) sb.Append(value[i]);
      return sb;
    }
    private enum NullHandling {
      ///<summary>the string "null", without the quotes</summary>
      LiteralNull,
      /// <summary>Change the null to the empty string, then wrap.</summary>
      EmptyContent,
      /// <summary>Return the empty string. Do not wrap.</summary>
      EmptyString,
    }
    private static StringBuilder AppendIn(this StringBuilder b, char l, string? s, char r, NullHandling h) =>
      h switch
      {
        NullHandling.EmptyContent => b.Append(l).Append(s).Append(r),
        NullHandling.EmptyString => s != null ? b.Append(l).Append(s).Append(r) : b,
        NullHandling.LiteralNull => b.Append(l).Append(s ?? "null").Append(r),
        _ =>
          throw new System.ComponentModel.InvalidEnumArgumentException(nameof(h), (int)h, typeof(NullHandling))
      };
    internal static StringBuilder AppendInBracesOrLiteralNull(this StringBuilder builder, string? appendMe) =>
      builder.AppendIn('{', appendMe, '}', NullHandling.LiteralNull);
    internal static StringBuilder AppendInBracesOrEmptyBraces(this StringBuilder builder, string? appendMe) =>
      builder.AppendIn('{', appendMe, '}', NullHandling.EmptyContent);
    internal static StringBuilder AppendInBracketsOrNothing(this StringBuilder builder, string? appendMe) =>
      builder.AppendIn('[', appendMe, ']', NullHandling.EmptyString);
    internal static StringBuilder AppendDebugStringOfScripts(this StringBuilder builder, Atom.MathAtom target) {
      if (target.Subscript.IsNonEmpty()) {
        builder.Append('_').AppendInBracesOrLiteralNull(target.Subscript.DebugString);
      }
      if (target.Superscript.IsNonEmpty()) {
        builder.Append('^').AppendInBracesOrLiteralNull(target.Superscript.DebugString);
      }
      return builder;
    }
  }
}
