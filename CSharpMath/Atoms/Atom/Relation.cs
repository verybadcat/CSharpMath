namespace CSharpMath.Atoms.Atom {
  /// <summary>A relation -- =, &lt; etc.</summary>
  public class Relation : MathAtom {
    public Relation(string nucleus) : base(nucleus) { }
    public override bool ScriptsAllowed => true;
    public new Relation Clone(bool finalize) => (Relation)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Relation(Nucleus);
  }
}