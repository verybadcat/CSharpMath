using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public static class MathAtoms {
    public static MathAtom Create(MathAtomType type, string value) {
      switch (type) {
        case MathAtomType.Accent:
          return new Accent(value);
        case MathAtomType.Color:
          return new MathColor();
        case MathAtomType.Fraction:
          return new Fraction();
        case MathAtomType.Inner:
          return new Inner();
        case MathAtomType.LargeOperator:
          return new LargeOperator(value, true);
        case MathAtomType.Overline:
          return new Overline();
        case MathAtomType.Underline:
          return new Underline();
        case MathAtomType.Space:
          return new MathSpace(0);
        default:
          return new MathAtom(type, value);
      }
    }
  }
}
