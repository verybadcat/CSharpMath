namespace CSharpMath.Atoms.Atom {
  public class Punctuation : MathAtom {
    public Punctuation(string nucleus) : base(nucleus) { }
    public override bool ScriptsAllowed => true;
    public new Punctuation Clone(bool finalize) => (Punctuation)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Punctuation(Nucleus);
  }
}