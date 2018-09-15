using CSharpMath.Enumerations;
using global::CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath {
  public static class StringBuilderMathExtensions {

    public static StringBuilder AppendNullAware(this StringBuilder builder, IStringValue appendMe, NullHandling handling)
      => builder.AppendNullAware(appendMe?.StringValue, handling);

    public static StringBuilder AppendInBraces(this StringBuilder builder, IStringValue appendMe, NullHandling handling)
      => builder.AppendInBraces(appendMe?.StringValue, handling);
    public static StringBuilder AppendInSquareBrackets(this StringBuilder builder, IStringValue appendMe, NullHandling handling)
      => builder.AppendInSquareBrackets(appendMe?.StringValue, handling);
    public static StringBuilder AppendInParens(this StringBuilder builder, IStringValue appendMe, NullHandling handling)
      => builder.AppendInParens(appendMe?.StringValue, handling);
    public static StringBuilder AppendScripts(this StringBuilder builder, IScripts target) {
      if (target.Superscript != null) {
        builder.AppendFormat("^{0}", target.Superscript.StringValue.WrapInBraces(NullHandling.LiteralNull));
      }
      if (target.Subscript != null) {
        builder.AppendFormat("_{0}", target.Subscript.StringValue.WrapInBraces(NullHandling.LiteralNull));
      }
      return builder;
    }
  }
}


