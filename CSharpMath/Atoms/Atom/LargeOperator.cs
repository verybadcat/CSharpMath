using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class LargeOperator : MathAtom {
    private bool _limits { get; set; }
    public bool Limits => _limits;
    public LargeOperator(string value, bool limits): base(MathAtomType.LargeOperator, value) {
      _limits = limits;
    }
    public LargeOperator(LargeOperator cloneMe, bool finalize): base(cloneMe, finalize) {
      _limits = cloneMe.Limits;
    }
  }
}
