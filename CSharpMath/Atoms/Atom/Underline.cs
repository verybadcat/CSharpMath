namespace CSharpMath.Atoms.Atom {
  /// <summary>An underlined atom</summary>
  public class Underline : MathAtom {
    public MathList InnerList { get; set; }
    public Underline(MathList innerList) : base() =>
      InnerList = innerList;
    public new Underline Clone(bool finalize) => (Underline)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Underline(InnerList.Clone(finalize));
    public override bool ScriptsAllowed => true;
    public override string DebugString =>
      new System.Text.StringBuilder(@"\underline")
      .AppendInBraces(InnerList.DebugString, NullHandling.LiteralNull)
      .ToString();
    public bool EqualsUnderline(Underline other) =>
      EqualsAtom(other) && InnerList.EqualsList(other.InnerList);
    public override bool Equals(object obj) =>
      obj is Underline u ? EqualsUnderline(u) : false;
    public override int GetHashCode() =>
      unchecked(base.GetHashCode() + 57 * InnerList.GetHashCode());
  }
}