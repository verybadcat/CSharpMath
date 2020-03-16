using System.Text;

namespace CSharpMath.Atom.Atoms {
  /// <summary>An inner atom, i.e. embedded math list</summary>
  public class Inner : MathAtom {
    public Inner(MathList innerList) : base(string.Empty) =>
      InnerList = innerList;
    public MathList InnerList { get; set; }
    public Boundary? LeftBoundary { get; set; }
    public Boundary? RightBoundary { get; set; }
    public override bool ScriptsAllowed => true;
    public new Inner Clone(bool finalize) => (Inner)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Inner(InnerList.Clone(finalize)) { 
        LeftBoundary = LeftBoundary,
        RightBoundary = RightBoundary
      };
    public bool EqualsInner(Inner otherInner) =>
      EqualsAtom(otherInner)
      && InnerList.NullCheckingStructuralEquality(otherInner.InnerList)
      && LeftBoundary.NullCheckingStructuralEquality(otherInner.LeftBoundary)
      && RightBoundary.NullCheckingStructuralEquality(otherInner.RightBoundary);
    public override bool Equals(object obj) => obj is Inner i ? EqualsInner(i) : false;
    public override int GetHashCode() =>
      unchecked(base.GetHashCode()
        + 23 * InnerList?.GetHashCode() ?? 0
        + 101 * LeftBoundary?.GetHashCode() ?? 0
        + 103 * RightBoundary?.GetHashCode() ?? 0);
    public override string DebugString =>
      new StringBuilder(@"\inner")
      .AppendInBracesOrEmptyBraces(LeftBoundary?.Nucleus)
      .AppendInBracesOrLiteralNull(InnerList?.DebugString)
      .AppendInBracesOrEmptyBraces(RightBoundary?.Nucleus)
      .AppendDebugStringOfScripts(this).ToString();
  }
}