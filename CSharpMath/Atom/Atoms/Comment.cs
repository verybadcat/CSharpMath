namespace CSharpMath.Atom.Atoms {
  public sealed class Comment : MathAtom {
    public Comment(string nucleus) : base(nucleus) { }
    public override bool ScriptsAllowed => false;
    public new Comment Clone(bool finalize) => (Comment)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Comment(Nucleus);
  }
}