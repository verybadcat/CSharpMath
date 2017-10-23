using global::CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath {
  public static class StringBuilderMathExtensions {

    public static void AppendNullAware(this StringBuilder builder, IStringValue appendMe, NullHandling handling)
      => builder.AppendNullAware(appendMe?.StringValue, handling);

    public static void AppendInBraces(this StringBuilder builder, IStringValue appendMe, NullHandling handling)
      => builder.AppendInBraces(appendMe?.StringValue, handling);
    public static void AppendInSquareBrackets(this StringBuilder builder, IStringValue appendMe, NullHandling handling)
      => builder.AppendInSquareBrackets(appendMe?.StringValue, handling);
    public static void AppendInParens(this StringBuilder builder, IStringValue appendMe, NullHandling handling)
      => builder.AppendInParens(appendMe?.StringValue, handling);
    public static void AppendScripts(this StringBuilder builder, IScripts target) {
      if (target.Superscript != null) {
        builder.AppendFormat($"^{target.Superscript.StringValue.WrapInBraces(NullHandling.LiteralNull)}");
      }
      if (target.Subscript != null) {
        builder.AppendFormat($"_{target.Subscript.StringValue.WrapInBraces(NullHandling.LiteralNull)}");
      }
    }
  }
}


