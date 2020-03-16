namespace CSharpMath.Atom.Atoms {
  /// <summary>Open brackets</summary>
  public class Open : MathAtom {
    public Open(string nucleus) : base(nucleus) { }
    public override bool ScriptsAllowed => true;
    public new Open Clone(bool finalize) => (Open)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Open(Nucleus);
  }
}
