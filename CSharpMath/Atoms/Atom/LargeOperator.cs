using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class LargeOperator : MathAtom {
    private bool _limits { get; set; }
    public bool Limits {
      get => _limits;
      set => _limits = value;
    }
    public LargeOperator(string value, bool limits): base(MathAtomType.LargeOperator, value) {
      _limits = limits;
    }

    public LargeOperator(LargeOperator cloneMe, bool finalize): base(cloneMe, finalize) {
      _limits = cloneMe.Limits;
    }

    public bool EqualsLargeOperator(LargeOperator obj) {
      bool r = this.EqualsAtom(obj);
      r &= (this.Limits == obj.Limits);
      return r;
    }

    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
  => visitor.Visit(this, helper);
  }
}
