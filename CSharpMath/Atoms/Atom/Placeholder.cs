using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;

namespace CSharpMath.Atoms.Atom {
  /// <summary>A placeholder for future input</summary>
  public class Placeholder : MathAtom {
    public Placeholder(string nucleus) : base(MathAtomType.Placeholder, nucleus) { }
    public override bool ScriptsAllowed => true;
    public new Placeholder Clone(bool finalize) => (Placeholder)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Placeholder(Nucleus);
  }
}
