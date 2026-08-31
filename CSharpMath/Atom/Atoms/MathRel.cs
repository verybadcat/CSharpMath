using System.Text;

namespace CSharpMath.Atom.Atoms {
  /// <summary>A complete subformula treated as one relation atom.</summary>
  public sealed class MathRel : MathAtom, IMathListContainer {
    public MathRel(MathList innerList) : base() => InnerList = innerList;
    public MathList InnerList { get; }
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
      new[] { InnerList };
    public override bool ScriptsAllowed => true;
    public new MathRel Clone(bool finalize) => (MathRel)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new MathRel(InnerList.Clone(finalize));
    public override string DebugString => new StringBuilder(@"\mathrel{")
      .Append(InnerList.DebugString).Append('}').AppendDebugStringOfScripts(this).ToString();
    public override bool Equals(object obj) => obj is MathRel other &&
      EqualsAtom(other) && InnerList.NullCheckingStructuralEquality(other.InnerList);
    public override int GetHashCode() => (base.GetHashCode(), InnerList).GetHashCode();
  }
}
