using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class AtomCloner: IMathAtomVisitor<IMathAtom, bool> {
    public IMathAtom Visit(MathAtom target, bool finalize) {
      return new MathAtom(target, finalize);
    }
    public IMathAtom Clone(IMathAtom target, bool finalize) {
      if (target == null) {
        return null;
      }
      return target.Accept(this, finalize);
    }
  }
}
