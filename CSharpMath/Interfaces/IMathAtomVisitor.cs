using CSharpMath.Atoms;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface IMathAtomVisitor<T, THelper> {
    T Visit(Accent accent, THelper helper);
    T Visit(Fraction fraction, THelper helper);
    T Visit(Inner inner, THelper helper);
    T Visit(LargeOperator op, THelper helper);
    T Visit(MathAtom atom, THelper helper);

    T Visit(MathColor color, THelper helper);
    T Visit(MathSpace space, THelper helper);
    T Visit(MathStyle style, THelper helper);
    T Visit(Table table, THelper helper);

    T Visit(Overline overline, THelper helper);
    T Visit(Underline underline, THelper helper);

    T Visit(Radical radical, THelper helper);


  }
}
