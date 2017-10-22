using CSharpMath.Atoms;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface IMathAtomVisitor<T, THelper> {
    T Visit(MathAtom atom, THelper helper);
  }
}
