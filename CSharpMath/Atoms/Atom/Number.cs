using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;

namespace CSharpMath.Atoms.Atom {
  public class Number : MathAtom {
    public Number(string number) : base(MathAtomType.Number, number) { }
    public override bool ScriptsAllowed => true;
    public new Number Clone(bool finalize) => (Number)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Number(Nucleus);
  }
}
