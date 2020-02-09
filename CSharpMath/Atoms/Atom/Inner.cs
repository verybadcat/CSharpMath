using CSharpMath.Enumerations;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms.Atom {
  /// <summary>An inner atom, i.e. embedded math list</summary>
  public class Inner : MathAtom {
    public Inner(MathList innerList) : base(MathAtomType.Inner, string.Empty) =>
      InnerList = innerList;
    public MathList InnerList { get; set; }
    private Boundary? _LeftBoundary;
    private Boundary? _RightBoundary;
    public Boundary? LeftBoundary {
      get => _LeftBoundary;
      set {
        if (value != null && value.AtomType!=MathAtomType.Boundary) {
          throw new InvalidOperationException("Boundary must have atom type Boundary");
        }
        _LeftBoundary = value;
      }
    }
    public Boundary? RightBoundary {
      get => _RightBoundary;
      set {
        if (value != null && value.AtomType != MathAtomType.Boundary) {
          throw new InvalidOperationException("Boundary must have atom type Boundary");
        }
        _RightBoundary = value;
      }
    }
    public override bool ScriptsAllowed => true;
    public new Inner Clone(bool finalize) => (Inner)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Inner(InnerList.Clone(finalize)) { 
        LeftBoundary = LeftBoundary?.Clone(finalize),
        RightBoundary = RightBoundary?.Clone(finalize)
      };

    public bool EqualsInner(Inner otherInner) =>
      EqualsAtom(otherInner)
      && InnerList.NullCheckingEquals(otherInner.InnerList)
      && LeftBoundary.NullCheckingEquals(otherInner.LeftBoundary)
      && RightBoundary.NullCheckingEquals(otherInner.RightBoundary);

    public override bool Equals(object obj) =>
      (obj is Inner) ? EqualsInner((Inner)obj) : false;

    public override int GetHashCode() =>
      unchecked(base.GetHashCode()
          + 23 * InnerList?.GetHashCode() ?? 0
          + 101 * LeftBoundary?.GetHashCode() ?? 0
          + 103 * RightBoundary?.GetHashCode() ?? 0);

    public override string DebugString =>
      new StringBuilder(@"\inner")
      .AppendInSquareBrackets(LeftBoundary?.Nucleus, NullHandling.EmptyString)
      .AppendInBraces(InnerList?.DebugString, NullHandling.LiteralNull)
      .AppendInBraces(RightBoundary?.Nucleus, NullHandling.EmptyString)
      .AppendDebugStringOfScripts(this).ToString();
  }
}
