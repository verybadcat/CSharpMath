using System.Text;

namespace CSharpMath.Atom.Atoms {
  /// <summary>An overlined atom</summary>
  public sealed class Overline : MathAtom, IMathListContainer {
    public Overline(MathList innerList) => InnerList = innerList;
    public MathList InnerList { get; }
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
      new[] { InnerList };
    public override bool ScriptsAllowed => true;
    public new Overline Clone(bool finalize) => (Overline)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Overline(InnerList.Clone(finalize));
    public override string DebugString =>
      new StringBuilder(@"\overline")
      .AppendInBracesOrLiteralNull(InnerList?.DebugString)
      .ToString();
    public bool EqualsOverline(Overline other) =>
      EqualsAtom(other) && InnerList.NullCheckingStructuralEquality(other?.InnerList);
    public override bool Equals(object obj) => obj is Overline o ? EqualsOverline(o) : false;
    public override int GetHashCode() =>
      (base.GetHashCode(), InnerList).GetHashCode();
  }
}