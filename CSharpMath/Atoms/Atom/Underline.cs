using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;
using CSharpMath.Interfaces;

namespace CSharpMath.Atoms {
  public class Underline : MathAtom, IUnderline {
    public IMathList InnerList { get; set; }
    public Underline() : base(MathAtomType.Underline, string.Empty, string.Empty) { }
    public Underline(Underline cloneMe, bool finalize) : base(cloneMe, finalize) {
      this.InnerList = AtomCloner.Clone(cloneMe.InnerList, finalize);
    }
    
    public override string StringValue =>
      new StringBuilder(@"\underline")
      .AppendInBraces(InnerList, NullHandling.LiteralNull)
      .ToString();

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

    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
=> visitor.Visit(this, helper);
  }
}
