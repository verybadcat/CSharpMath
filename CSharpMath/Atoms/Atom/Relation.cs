using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;

namespace CSharpMath.Atoms.Atom {
  /// <summary>A relation -- =, &lt; etc.</summary>
  public class Relation : MathAtom {
    public Relation(string nucleus) : base(MathAtomType.Relation, nucleus) { }
    public override bool ScriptsAllowed => true;
    public new Relation Clone(bool finalize) => (Relation)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Relation(Nucleus);
  }
}
