using System;
using System.Text;

namespace CSharpMath {
  using Enumerations;
  using Interfaces;
  public static class StringBuilderExtensions {
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
    public static StringBuilder AppendUnlessNull(this StringBuilder builder,
      string appendMe, string? unlessNull) =>
      unlessNull != null
      ? builder.Append(appendMe)
      : builder;
    public static StringBuilder AppendNullAware(this StringBuilder builder,
      string? appendMe, NullHandling handling) =>
      builder.Append(appendMe.NullToNull(handling));
    public static StringBuilder AppendInBraces(this StringBuilder builder,
      string? appendMe, NullHandling handling) =>
      builder.Append(appendMe.WrapInBraces(handling));
    public static StringBuilder AppendInSquareBrackets(this StringBuilder builder,
      string? appendMe, NullHandling handling) =>
      builder.Append(appendMe.WrapInSquareBrackets(handling));
    public static StringBuilder AppendInParens(this StringBuilder builder,
      string? appendMe, NullHandling handling) =>
      builder.Append(appendMe.WrapInParens(handling));
    public static StringBuilder AppendDebugStringOfScripts
      (this StringBuilder builder, IScripts target) {
      if (target.Superscript != null) {
        builder.AppendFormat("^{0}",
          target.Superscript.DebugString.WrapInBraces(NullHandling.LiteralNull));
      }
      if (target.Subscript != null) {
        builder.AppendFormat("_{0}",
          target.Subscript.DebugString.WrapInBraces(NullHandling.LiteralNull));
      }
      return builder;
    }
  }
}
