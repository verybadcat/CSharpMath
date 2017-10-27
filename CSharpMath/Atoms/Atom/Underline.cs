using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Interfaces;

namespace CSharpMath.Atoms {
  public class Underline : MathAtom, IOverline {
    public IMathList InnerList { get; set; }
    public Underline() : base(MathAtomType.Underline, "") { }
    public Underline(Underline cloneMe, bool finalize) : base(cloneMe, finalize) {
      this.InnerList = AtomCloner.Clone(cloneMe.InnerList, finalize);

    }
  }
}
