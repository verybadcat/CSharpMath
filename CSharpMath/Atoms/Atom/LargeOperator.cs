using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class LargeOperator : MathAtom {
    public bool Limits { get; set; }

    public LargeOperator(string value, bool limits): base(MathAtomType.LargeOperator, value) {
      Limits = limits;
    }

    public LargeOperator(LargeOperator cloneMe, bool finalize): base(cloneMe, finalize) {
      Limits = cloneMe.Limits;
    }

    public override string StringValue => base.StringValue + (Limits ? @"\limits" : @"\nolimits");

    public bool EqualsLargeOperator(LargeOperator obj) =>
      EqualsAtom(obj)
      && Limits == obj.Limits;

    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
  => visitor.Visit(this, helper);
  }
}
