namespace CSharpMath.Atom.Atoms {
  /// <summary>A unary operator</summary>
  public class UnaryOperator : MathAtom {
    public UnaryOperator(string nucleus) : base(nucleus) { }
    public override bool ScriptsAllowed => true;
    public new UnaryOperator Clone(bool finalize) => (UnaryOperator)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new UnaryOperator(Nucleus);
    public Ordinary ToOrdinary() => ApplyCommonPropertiesOn(false, new Ordinary(Nucleus));
  }
}