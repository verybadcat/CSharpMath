namespace CSharpMath.Atom.Atoms {
  public class RaiseBox : MathAtom, IMathListContainer1 {
    public Structures.Space Raise { get; set; }
    public MathList? InnerList { get; set; }
    public override bool ScriptsAllowed => false;
    public new RaiseBox Clone(bool finalize) => (RaiseBox)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new RaiseBox { Raise = Raise, InnerList = InnerList?.Clone(finalize) };
  }
}