using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class AtomCloner: IMathAtomVisitor<IMathAtom, bool> {
    public static AtomCloner Instance { get; } = new AtomCloner();
   public IMathAtom Visit(Accent target, bool finalize)
  => new Accent(target, finalize);
    public IMathAtom Visit(Fraction target, bool finalize)
  => new Fraction(target, finalize);

    public IMathAtom Visit(Inner target, bool finalize)
  => new Inner(target, finalize);

    public IMathAtom Visit(LargeOperator target, bool finalize)
      => new LargeOperator(target, finalize);
    public IMathAtom Visit(MathAtom target, bool finalize)
      => new MathAtom(target, finalize);

    public IMathAtom Visit(MathColor target, bool finalize)
      => new MathColor(target, finalize);
    public IMathAtom Visit(MathSpace target, bool finalize)
      => new MathSpace(target, finalize);

    public IMathAtom Visit(MathTable target, bool finalize)
      => new MathTable(target, finalize);

    public IMathAtom Visit(MathStyle target, bool finalize)
      => new MathStyle(target, finalize);

    public IMathAtom Visit(Overline target, bool finalize)
      => new Overline(target, finalize);

    public IMathAtom Visit(Radical target, bool finalize)
      => new Radical(target, finalize);

    public IMathAtom Visit(Underline target, bool finalize)
      => new Underline(target, finalize);
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
