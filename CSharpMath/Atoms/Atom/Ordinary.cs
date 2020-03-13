namespace CSharpMath.Atoms.Atom {
  public class Ordinary : MathAtom {
    public Ordinary(string nucleus) : base(nucleus) { }
    public override bool ScriptsAllowed => true;
    public new Ordinary Clone(bool finalize) => (Ordinary)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Ordinary(Nucleus);
  }
}