using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Interfaces;

namespace CSharpMath.Atoms {
  public class Overline : MathAtom, IOverline {
    public IMathList InnerList { get; set; }
    public Overline() : base(MathAtomType.Overline, "") { }
    public Overline(Overline cloneMe, bool finalize) : base(cloneMe, finalize) {
      this.InnerList = AtomCloner.Clone(cloneMe.InnerList, finalize);
    }
  }
}
