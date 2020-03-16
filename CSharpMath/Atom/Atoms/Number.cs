namespace CSharpMath.Atom.Atoms {
  public class Number : MathAtom {
    public Number(string number) : base(number) { }
    public override bool ScriptsAllowed => true;
    public new Number Clone(bool finalize) => (Number)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Number(Nucleus);
    public Ordinary ToOrdinary(
      System.Func<string, FontStyle, string> fontChanger) =>
      ApplyCommonPropertiesOn(false, new Ordinary(fontChanger(Nucleus, FontStyle)));
  }
}