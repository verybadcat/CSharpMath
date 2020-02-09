using System.Text;

namespace CSharpMath.Atoms.Atom {
  /// <summary>An accented atom</summary>
  public class Accent : MathAtom {
    public MathList? InnerList { get; set; }
    public Accent(string value) : base(value) { }
    public override string DebugString =>
      new StringBuilder(@"\accent")
      .AppendInBraces(Nucleus, NullHandling.LiteralNull)
      .AppendInBraces(InnerList?.DebugString, NullHandling.LiteralNull)
      .ToString();
    public new Accent Clone(bool finalize) => (Accent)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Accent(Nucleus) {
      InnerList = InnerList?.Clone(finalize)
    };
    public override bool ScriptsAllowed => true;
    public bool EqualsAccent(Accent other) =>
      EqualsAtom(other) && InnerList.NullCheckingEquals(other?.InnerList);
    public override bool Equals(object obj) => obj is Accent a ? EqualsAccent(a) : false;
    public override int GetHashCode() =>
      unchecked(base.GetHashCode() + 71 * InnerList?.GetHashCode() ?? 1);
  }
}