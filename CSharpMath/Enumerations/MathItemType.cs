using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath.Enumerations {
  public enum MathAtomType {
    MinValue = 0,
    Ordinary = 1,
    Number,
    Variable,
    /// <summary>
    /// A large operator such integral, etc.
    /// </summary>
    LargeOperator,
    /// <summary>
    /// An operator such as sin/cos etc.
    /// </summary>
    Operator,
    /// <summary>
    /// A binary operator
    /// </summary>
    BinaryOperator,
    /// <summary>
    /// A unary operator 
    /// </summary>
    UnaryOperator,
    /// <summary>
    /// A relation -- =, &lt; etc.
    /// </summary>
    Relation,
    /// <summary>
    /// Open brackets
    /// </summary>
    Open,
    /// <summary>
    /// Close brackets
    /// </summary>
    Close,
    Fraction,
    Radical,
    Punctuation,
    /// <summary>
    /// A placeholder for future input
    /// </summary>
    Placeholder,
    /// <summary>
    /// An inner atom, i.e. embedded math list
    /// </summary>
    Inner,
    /// <summary>
    /// An underlined atom
    /// </summary>
    Underline,
    /// <summary>
    /// An overlined atom
    /// </summary>
    Overline,
    /// <summary>
    /// An accented atom
    /// </summary>
    Accent,

    #region Atoms not in iosMath (aka in the Extension)

    Group = 50,
    RaiseBox,
    Prime,

    #endregion


    //Atoms after this point do not support subscripts or superscripts.

    ///<summary>A left atom -- Left and Right in TeX. We don't need two since we track boundaries separately.</summary>
    Boundary = 101,

    //Atoms after this point are non-math nodes that are still useful in math mode. 
    // They do not have the usual structure.

    Space=201,
    ///<summary>Style changes during rendering</summary>
    Style,
    Color,
    ///<summary>A table. Not part of TeX.</summary>
    Table = 1001
  }
}
