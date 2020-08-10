namespace CSharpMath.Atom.Atoms {
  /// <summary>AMSMath class 2: Binary Operator (conjunction) when no suitable left operand
  /// (at beginning or after open), typesetted as class 0: Ordinary to remove spacing</summary>
  public sealed class UnaryOperator : MathAtom {
    public UnaryOperator(string nucleus) : base(nucleus) { }
    public override bool ScriptsAllowed => true;
    public new UnaryOperator Clone(bool finalize) => (UnaryOperator)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new UnaryOperator(Nucleus);
    public Ordinary ToOrdinary() => ApplyCommonPropertiesOn(false, new Ordinary(Nucleus));
  }
}