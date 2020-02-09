using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;

namespace CSharpMath.Atoms.Atom {
  ///<summary>A left atom -- Left and Right in TeX.
  ///We don't need two since we track boundaries separately.</summary>
  public class Boundary : MathAtom {
    public Boundary(string nucleus) : base(MathAtomType.Boundary, nucleus) { }
    public override bool ScriptsAllowed => false;
    public new Boundary Clone(bool finalize) => (Boundary)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Boundary(Nucleus);
  }
}
