namespace CSharpMath.Atom.Atoms {
  public sealed class Variable : MathAtom {
    public Variable(string variable) : base(variable) { }
    public override bool ScriptsAllowed => true;
    public new Variable Clone(bool finalize) => (Variable)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Variable(Nucleus);
    public Ordinary ToOrdinary(
      System.Func<string, FontStyle, string> fontChanger) =>
      ApplyCommonPropertiesOn(false, new Ordinary(fontChanger(Nucleus, FontStyle)));
  }
}