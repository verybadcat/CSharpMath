using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath {
  public static class StringBuilderExtensions {
    public static void AppendUnlessNull(this StringBuilder builder, string appendMe, string unlessNull) {
      if (unlessNull!=null) {
        builder.Append(appendMe);
      }
    }
    public static void AppendNullAware(this StringBuilder builder, string appendMe, NullHandling handling) 
      => builder.Append(appendMe.NullToNull(handling));

    public static void AppendInBraces(this StringBuilder builder, string appendMe, NullHandling handling)
      => builder.Append(appendMe.WrapInBraces(handling));
    public static void AppendInSquareBrackets(this StringBuilder builder, string appendMe, NullHandling handling)
      => builder.Append(appendMe.WrapInSquareBrackets(handling));
    public static void AppendInParens(this StringBuilder builder, string appendMe, NullHandling handling)
      => builder.Append(appendMe.WrapInParens(handling));
    
  }
}
