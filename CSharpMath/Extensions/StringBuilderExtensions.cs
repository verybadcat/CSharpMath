using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath {
  using Enumerations;
  public static class StringBuilderExtensions {
    public static StringBuilder Append(this StringBuilder sb1, StringBuilder sb2) {
      sb1.EnsureCapacity(sb1.Length + sb2.Length);
      for (int i = 0; i < sb2.Length; i++) sb1.Append(sb2[i]);
      return sb1;
    }
    public static StringBuilder AppendUnlessNull(this StringBuilder builder, string appendMe, string unlessNull) {
      if (unlessNull!=null) {
        return builder.Append(appendMe);
      }
      return builder;
    }
    public static StringBuilder AppendNullAware(this StringBuilder builder, string appendMe, NullHandling handling) 
      => builder.Append(appendMe.NullToNull(handling));

    public static StringBuilder AppendInBraces(this StringBuilder builder, string appendMe, NullHandling handling)
      => builder.Append(appendMe.WrapInBraces(handling));
    public static StringBuilder AppendInSquareBrackets(this StringBuilder builder, string appendMe, NullHandling handling)
      => builder.Append(appendMe.WrapInSquareBrackets(handling));
    public static StringBuilder AppendInParens(this StringBuilder builder, string appendMe, NullHandling handling)
      => builder.Append(appendMe.WrapInParens(handling));
    
  }
}
