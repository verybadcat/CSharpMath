using System;
using System.Text;

namespace CSharpMath {
  public static partial class Extensions {
    public static StringBuilder Append(this StringBuilder sb, ReadOnlySpan<char> value) {
      sb.EnsureCapacity(sb.Length + value.Length);
      for (int i = 0; i < value.Length; i++) sb.Append(value[i]);
      return sb;
    }
    public static StringBuilder Append(this StringBuilder sb1, StringBuilder sb2) {
      sb1.EnsureCapacity(sb1.Length + sb2.Length);
      for (int i = 0; i < sb2.Length; i++) sb1.Append(sb2[i]);
      return sb1;
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
    public static StringBuilder AppendInBracesOrLiteralNull(this StringBuilder builder, string? appendMe) =>
      builder.AppendIn('{', appendMe, '}', NullHandling.LiteralNull);
    public static StringBuilder AppendInBracesOrEmptyBraces(this StringBuilder builder, string? appendMe) =>
      builder.AppendIn('{', appendMe, '}', NullHandling.EmptyContent);
    public static StringBuilder AppendInBracketsOrNothing(this StringBuilder builder, string? appendMe) =>
      builder.AppendIn('[', appendMe, ']', NullHandling.EmptyString);
    public static StringBuilder AppendDebugStringOfScripts(this StringBuilder builder, Atom.MathAtom target) {
      if (target.Subscript != null) {
        builder.Append('_').AppendInBracesOrLiteralNull(target.Subscript.DebugString);
      }
      if (target.Superscript != null) {
        builder.Append('^').AppendInBracesOrLiteralNull(target.Superscript.DebugString);
      }
      return builder;
    }
  }
}