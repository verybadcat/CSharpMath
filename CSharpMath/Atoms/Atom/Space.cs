using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class Space : MathAtom, ISpace {
    public float Length { get; }

    //is the length in math units (mu) or points (pt)?
    public bool IsMu { get; }

    bool IsBreakable { get; }

    public Space(float length, bool isMu, bool isBreakable = true) : base(MathAtomType.Space, string.Empty) {
      Length = length;
      IsMu = isMu;
      IsBreakable = isBreakable;
    }

    public Space(Space cloneMe, bool finalize) : base(cloneMe, finalize) {
      Length = cloneMe.Length;
      IsMu = cloneMe.IsMu;
    }

    public bool EqualsSpace(Space otherSpace) =>
      EqualsAtom(otherSpace)
      && Length == otherSpace.Length
      && IsMu == otherSpace.IsMu;

    public override int GetHashCode() {
      unchecked {
        return base.GetHashCode() + 73 * Length.GetHashCode() + 277 * IsMu.GetHashCode();
      }
    }

    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
      => visitor.Visit(this, helper);
  }
}