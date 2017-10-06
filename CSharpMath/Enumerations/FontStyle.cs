using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Enumerations {
  public enum FontStyle {
    Default = 0,
    ///<summary>\mathrm</summary>
    Roman,
    Bold,
    Caligraphic,
    ///<summary>Monospace, i.e.</summary>
    Typewriter,
    ///<summary>\mathit</summary>
    Italic,
    ///<summary>\mathss</summary>
    SansSerif,
    ///<summary>\mathfrak</summary>
    Fraktur,
    ///<summary>\mathbb</summary>
    Blackboard,
    BoldItalic
  }
}
