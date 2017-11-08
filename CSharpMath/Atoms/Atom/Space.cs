using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class MathSpace : MathAtom, ISpace {
    private readonly float _space; // mu units
    public float Space => _space;

    public MathSpace(float space): base(MathAtomType.Space, "") {
      _space = space;
    }

    public MathSpace(MathSpace cloneMe, bool finalize): base(cloneMe, finalize) {
      _space = cloneMe._space;
    }

    public bool EqualsSpace(MathSpace otherSpace) {
      bool r = EqualsAtom(otherSpace);
      r &= Space == otherSpace.Space;
      return r;
    }

    public override int GetHashCode() {
      unchecked {
        return base.GetHashCode() + 73 * _space.GetHashCode();
      }
    }

    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
      => visitor.Visit(this, helper);
  }
}
