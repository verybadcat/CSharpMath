using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Interfaces;

namespace CSharpMath.Atoms {
  public class Group : MathAtom {
    public IMathList InnerList { get; set; }
    public Group() : base(MathAtomType.Group, string.Empty) {

    }
    public Group(Group cloneMe, bool finalize) : base(cloneMe, finalize) {
      InnerList = AtomCloner.Clone(cloneMe.InnerList, finalize);
      Nucleus = cloneMe.Nucleus;
      AtomType = MathAtomType.Accent;
    }
    public override string StringValue => InnerList.StringValue;
    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper) => visitor.Visit(this, helper);
    public override string ToString() => StringValue;
  }
}
