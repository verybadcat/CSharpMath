using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;
using CSharpMath.Interfaces;

namespace CSharpMath.Atoms.Atom {
  public class RaiseBox : MathAtom {
    public Space Raise { get; }
    public MathList InnerList { get; set; }
    public RaiseBox(Space raise, MathList innerList) : base(MathAtomType.RaiseBox) =>
      (Raise, InnerList) = (raise, innerList);
    public override bool ScriptsAllowed => false;
    public new RaiseBox Clone(bool finalize) => (RaiseBox)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new RaiseBox(Raise, InnerList.Clone(finalize));
  }
}
