using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;

namespace CSharpMath.Atoms.Atom {
  /// <summary>A unary operator</summary>
  public class UnaryOperator : MathAtom {
    public UnaryOperator(string nucleus) : base(MathAtomType.UnaryOperator, nucleus) { }
    public override bool ScriptsAllowed => true;
    public new UnaryOperator Clone(bool finalize) => (UnaryOperator)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new UnaryOperator(Nucleus);
  }
}
