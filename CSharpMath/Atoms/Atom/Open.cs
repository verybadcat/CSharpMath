using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;

namespace CSharpMath.Atoms.Atom {
  /// <summary>Open brackets</summary>
  public class Open : MathAtom {
    public Open(string nucleus) : base(MathAtomType.Open, nucleus) { }
    public override bool ScriptsAllowed => true;
    public new Open Clone(bool finalize) => (Open)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Open(Nucleus);
  }
}
