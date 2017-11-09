using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class Inner : MathAtom,IMathInner {
    public Inner(): base(MathAtomType.Inner, "") {

    }
    private IMathAtom _LeftBoundary;
    private IMathAtom _RightBoundary;
    public IMathList InnerList { get; set; }
    public IMathAtom LeftBoundary {
      get => _LeftBoundary;
      set {
        if (value != null && value.AtomType!=MathAtomType.Boundary) {
          throw new InvalidOperationException("Boundary must have atom type Boundary");
        }
        _LeftBoundary = value;
      }
    }
    public IMathAtom RightBoundary {
      get => _RightBoundary;
      set {
        if (value != null && value.AtomType != MathAtomType.Boundary) {
          throw new InvalidOperationException("Boundary must have atom type Boundary");
        }
        _RightBoundary = value;
      }
    }

    public Inner(Inner cloneMe, bool finalize): base(cloneMe, finalize) {
      InnerList = AtomCloner.Clone(cloneMe.InnerList, finalize);
      LeftBoundary = AtomCloner.Clone(cloneMe.LeftBoundary, finalize);
      RightBoundary = AtomCloner.Clone(cloneMe.RightBoundary, finalize);
    }

    public bool EqualsInner(Inner otherInner) {
      bool r = EqualsAtom(otherInner);
      r &= InnerList.NullCheckingEquals(otherInner.InnerList);
      r &= LeftBoundary.NullCheckingEquals(otherInner.LeftBoundary);
      r &= RightBoundary.NullCheckingEquals(otherInner.RightBoundary);
      return r;
    }

    public override bool Equals(object obj) =>
      (obj is Inner) ? EqualsInner((Inner)obj) : false;

    public override int GetHashCode() {
      unchecked {
        return base.GetHashCode()
          + 23 * InnerList?.GetHashCode() ?? 0
          + 101 * LeftBoundary?.GetHashCode() ?? 0
          + 103 * RightBoundary?.GetHashCode() ?? 0;
      }
    }

    public override string StringValue {
      get {
        var builder = new StringBuilder(@"\inner");
        builder.AppendInSquareBrackets(LeftBoundary.Nucleus, NullHandling.EmptyString);
        builder.AppendInBraces(InnerList, NullHandling.LiteralNull);
        builder.AppendScripts(this);
        var r = builder.ToString();
        return r;
      }
    }

    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
  => visitor.Visit(this, helper);
  }
}
