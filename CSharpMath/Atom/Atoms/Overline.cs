using System.Text;

namespace CSharpMath.Atom.Atoms {
  /// <summary>An overlined atom</summary>
  public class Overline : MathAtom, IMathListContainer1 {
    public MathList? InnerList { get; set; }
    public override bool ScriptsAllowed => true;
    public new Overline Clone(bool finalize) => (Overline)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Overline { InnerList = InnerList?.Clone(finalize) };
    public override string DebugString =>
      new StringBuilder(@"\overline")
      .AppendInBracesOrLiteralNull(InnerList?.DebugString)
      .ToString();
    public bool EqualsOverline(Overline other) =>
      EqualsAtom(other) && InnerList.NullCheckingStructuralEquality(other?.InnerList);
    public override bool Equals(object obj) => obj is Overline o ? EqualsOverline(o) : false;
    public override int GetHashCode() =>
      unchecked(base.GetHashCode() + 53 * InnerList?.GetHashCode() ?? 0);
  }
}