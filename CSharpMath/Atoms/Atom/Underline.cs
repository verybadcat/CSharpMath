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

    public bool EqualsUnderline(Underline other) {
      bool r = this.EqualsAtom(other);
      r &= InnerList.NullCheckingEquals(other.InnerList);
      return r;
    }

    public override bool Equals(object obj)
      => EqualsUnderline(obj as Underline);

    public override int GetHashCode() {
      unchecked {
        return base.GetHashCode()
        + 57 * InnerList?.GetHashCode() ?? 1;
      }
    }
  }
}
