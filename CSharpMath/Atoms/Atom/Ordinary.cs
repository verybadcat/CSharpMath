using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;

namespace CSharpMath.Atoms.Atom {
  public class Ordinary : MathAtom {
    public Ordinary(string nucleus) : base(MathAtomType.Ordinary, nucleus) { }
    public override bool ScriptsAllowed => true;
    public new Ordinary Clone(bool finalize) => (Ordinary)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Ordinary(Nucleus);
  }
}
