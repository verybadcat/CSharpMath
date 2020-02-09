using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;

namespace CSharpMath.Atoms.Atom {
  public class Variable : MathAtom {
    public Variable(string variable) : base(MathAtomType.Variable, variable) { }
    public override bool ScriptsAllowed => true;
    public new Variable Clone(bool finalize) => (Variable)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Variable(Nucleus);
  }
}
