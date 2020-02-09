namespace CSharpMath.Atoms.Atom {
  /// <summary>A binary operator</summary>
  public class BinaryOperator : MathAtom {
    public BinaryOperator(string nucleus) : base(nucleus) { }
    public override bool ScriptsAllowed => true;
    public new BinaryOperator Clone(bool finalize) => (BinaryOperator)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new BinaryOperator(Nucleus);
    public UnaryOperator ToUnaryOperator() =>
      ApplyCommonPropertiesOn(false, new UnaryOperator(Nucleus));
  }
}