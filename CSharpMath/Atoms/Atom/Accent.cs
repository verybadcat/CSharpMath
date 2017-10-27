using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class Accent : MathAtom, IAccent {
    public IMathList InnerList { get; set; }
    public Accent(string value): base(MathAtomType.Accent, value) { }

    public Accent(Accent cloneMe, bool finalize): base(cloneMe, finalize) {
      InnerList = AtomCloner.Clone(cloneMe.InnerList, finalize);
      Nucleus = cloneMe.Nucleus;
    }
  }
}
