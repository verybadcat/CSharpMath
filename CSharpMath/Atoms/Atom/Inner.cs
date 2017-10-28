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
        if (value.AtomType!=MathAtomType.Boundary) {
          throw new InvalidOperationException("Boundary must have atom type Boundary");
        }
        _LeftBoundary = value;
      }
    }
    public IMathAtom RightBoundary {
      get => _RightBoundary;
      set {
        if (value.AtomType != MathAtomType.Boundary) {
          throw new InvalidOperationException("Boundary must have atom type Boundary");
        }
        _RightBoundary = value;
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
  }
}
