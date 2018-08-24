using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Enumerations {
  public enum NullHandling {
    LiteralNull, // the string "null", without the quotes
    EmptyContent, // Change the null to the empty string, then do whatever.
    EmptyString, // Return the empty string. Do not wrap.
    None, // null remains null.
  }
}
