namespace CSharpMath.Atom.Atoms {
  public class RaiseBox : MathAtom {
    public Space Raise { get; }
    public MathList InnerList { get; set; }
    public RaiseBox(Space raise, MathList innerList) : base() =>
      (Raise, InnerList) = (raise, innerList);
    public override bool ScriptsAllowed => false;
    public new RaiseBox Clone(bool finalize) => (RaiseBox)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new RaiseBox(Raise, InnerList.Clone(finalize));
  }
}