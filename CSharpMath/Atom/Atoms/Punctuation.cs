namespace CSharpMath.Atom.Atoms {
  /// <summary>AMSMath class 6: Postfix/Punctuation, e.g. . , ; !</summary>
  public sealed class Punctuation : MathAtom {
    public Punctuation(string nucleus) : base(nucleus) { }
    public override bool ScriptsAllowed => true;
    public new Punctuation Clone(bool finalize) => (Punctuation)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Punctuation(Nucleus);
  }
}