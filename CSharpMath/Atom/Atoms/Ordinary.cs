namespace CSharpMath.Atom.Atoms {
  public sealed class Ordinary : MathAtom {
    public Ordinary(string nucleus) : base(nucleus) { }
    public override bool ScriptsAllowed => true;
    public new Ordinary Clone(bool finalize) => (Ordinary)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Ordinary(Nucleus);
  }
}