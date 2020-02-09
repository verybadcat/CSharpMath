namespace CSharpMath.Enumerations {
  public enum MathAtomType {
    MinValue = 0,
    Ordinary = 1,
    Number,
    Variable,
    LargeOperator,
    BinaryOperator,
    UnaryOperator,
    Relation,
    Open,
    Close,
    Fraction,
    Radical,
    Punctuation,
    Placeholder,
    Inner,
    Underline,
    Overline,
    Accent,

    #region Atoms not in iosMath (aka in the Extension)

    Group = 50,
    RaiseBox,
    Prime,

    #endregion


    //Atoms after this point do not support subscripts or superscripts.

    Boundary = 101,

    //Atoms after this point are non-math nodes that are still useful in math mode. 
    // They do not have the usual structure.

    Space=201,
    Style,
    Color,
    Table = 1001
  }
}