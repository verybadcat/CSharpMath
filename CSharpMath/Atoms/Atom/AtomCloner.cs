using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class AtomCloner: IMathAtomVisitor<IMathAtom, bool> {
    public static AtomCloner Instance { get; } = new AtomCloner();
    public IMathAtom Visit(MathAtom target, bool finalize) {
      return new MathAtom(target, finalize);
    }
    public static IMathAtom Clone(IMathAtom target, bool finalize) {
      if (target == null) {
        return null;
      }
      return target.Accept(Instance, finalize);
    }
    public static IMathList Clone(IMathList target, bool finalize) {
      if (target == null) {
        return null;
      }
      return new MathList((MathList)target, finalize);
    }
  }
}
