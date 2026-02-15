namespace CSharpMath.Atom.Atoms {
  public sealed class RaiseBox : MathAtom, IMathListContainer {
    public RaiseBox(Atom.Length raise, MathList innerList) =>
      (Raise, InnerList) = (raise, innerList);
    public Atom.Length Raise { get; }
    public MathList InnerList { get; }
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
      new[] { InnerList };
    public override bool ScriptsAllowed => false;
    public new RaiseBox Clone(bool finalize) => (RaiseBox)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new RaiseBox(Raise, InnerList.Clone(finalize));
    public override int GetHashCode() =>
      (base.GetHashCode(), Raise, InnerList).GetHashCode();
    public override bool Equals(object obj) =>
      obj is RaiseBox r ? EqualsAtom(r) && Raise == r.Raise
      && InnerList.NullCheckingStructuralEquality(r.InnerList) : false;
  }
}