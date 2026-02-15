using System.Text;

namespace CSharpMath.Atom.Atoms {
  /// <summary>An accented atom</summary>
  public sealed class Accent : MathAtom, IMathListContainer {
    public MathList InnerList { get; } = new MathList();
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
      new[] { InnerList };
    public Accent(string value, MathList? innerList = null) : base(value) =>
      InnerList = innerList ?? new MathList();
    public override string DebugString =>
      new StringBuilder(@"\accent")
      .AppendInBracesOrLiteralNull(Nucleus)
      .AppendInBracesOrLiteralNull(InnerList?.DebugString)
      .ToString();
    public new Accent Clone(bool finalize) => (Accent)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Accent(Nucleus, InnerList.Clone(finalize));
    public override bool ScriptsAllowed => true;
    public bool EqualsAccent(Accent other) =>
      EqualsAtom(other) && InnerList.NullCheckingStructuralEquality(other?.InnerList);
    public override bool Equals(object obj) => obj is Accent a ? EqualsAccent(a) : false;
    public override int GetHashCode() => (base.GetHashCode(), InnerList).GetHashCode();
  }
}