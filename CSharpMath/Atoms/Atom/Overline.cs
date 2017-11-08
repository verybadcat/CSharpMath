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

    public bool EqualsOverline(Overline other) {
      bool r = this.EqualsAtom(other);
      r &= InnerList.NullCheckingEquals(other.InnerList);
      return r;
    }

    public override bool Equals(object obj)
      => EqualsOverline(obj as Overline);

    public override int GetHashCode() {
      unchecked {
        return base.GetHashCode()
        + 53 * InnerList?.GetHashCode() ?? 1;
      }
    }

    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
=> visitor.Visit(this, helper);
  }
}
