using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Enumerations {
  public enum NullHandling {
    ///<summary>the string "null", without the quotes</summary>
    LiteralNull,
    /// <summary>Change the null to the empty string, then wrap.</summary>
    EmptyContent,
    /// <summary>Return the empty string. Do not wrap.</summary>
    EmptyString,
  }
}
