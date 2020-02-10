using System.Text;

namespace CSharpMath.Atoms.Atom {
  /// <summary>An overlined atom</summary>
  public class Overline : MathAtom {
    public MathList InnerList { get; set; }
    public Overline(MathList innerList) : base(string.Empty) =>
      InnerList = innerList;
    public override bool ScriptsAllowed => true;
    public new Overline Clone(bool finalize) => (Overline)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Overline(InnerList.Clone(finalize));
    public override string DebugString =>
      new StringBuilder(@"\overline")
      .AppendInBracesOrLiteralNull(InnerList.DebugString)
      .ToString();
    public bool EqualsOverline(Overline other) =>
      EqualsAtom(other) && InnerList.NullCheckingEquals(other?.InnerList);
    public override bool Equals(object obj) => obj is Overline o ? EqualsOverline(o) : false;
    public override int GetHashCode() =>
      unchecked(base.GetHashCode() + 53 * InnerList.GetHashCode());
  }
}