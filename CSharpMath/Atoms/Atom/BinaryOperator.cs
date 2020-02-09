using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;

namespace CSharpMath.Atoms.Atom {
  /// <summary>A binary operator</summary>
  public class BinaryOperator : MathAtom {
    public BinaryOperator(string nucleus) : base(MathAtomType.BinaryOperator, nucleus) { }
    public override bool ScriptsAllowed => true;
    public new BinaryOperator Clone(bool finalize) => (BinaryOperator)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new BinaryOperator(Nucleus);
  }
}
