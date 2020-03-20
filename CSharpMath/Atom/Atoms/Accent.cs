using System.Text;

namespace CSharpMath.Atom.Atoms {
  /// <summary>An accented atom</summary>
  public class Accent : MathAtom, IMathListContainer1 {
    public MathList? InnerList { get; set; }
    public Accent(string value) : base(value) { }
    public override string DebugString =>
      new StringBuilder(@"\accent")
      .AppendInBracesOrLiteralNull(Nucleus)
      .AppendInBracesOrLiteralNull(InnerList?.DebugString)
      .ToString();
    public new Accent Clone(bool finalize) => (Accent)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Accent(Nucleus) {
      InnerList = InnerList?.Clone(finalize)
    };
    public override bool ScriptsAllowed => true;
    public bool EqualsAccent(Accent other) =>
      EqualsAtom(other) && InnerList.NullCheckingStructuralEquality(other?.InnerList);
    public override bool Equals(object obj) => obj is Accent a ? EqualsAccent(a) : false;
    public override int GetHashCode() =>
      unchecked(base.GetHashCode() + 71 * InnerList?.GetHashCode() ?? 1);
  }
}