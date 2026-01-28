using System.Text;

namespace CSharpMath.Atom.Atoms {
  public sealed class Fraction : MathAtom, IMathListContainer {
    public MathList Numerator { get; }
    public MathList Denominator { get; }
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
      new[] { Numerator, Denominator };
    public Boundary LeftDelimiter { get; set; }
    public Boundary RightDelimiter { get; set; }
    /// <summary>In this context, a "rule" is a fraction line.</summary>
    public bool HasRule { get; }
    public Fraction(MathList numerator, MathList denominator, bool hasRule = true) =>
      (Numerator, Denominator, HasRule) = (numerator, denominator, hasRule);
    public override bool ScriptsAllowed => true;
    public new Fraction Clone(bool finalize) => (Fraction)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Fraction(Numerator.Clone(finalize), Denominator.Clone(finalize), HasRule) {
        LeftDelimiter = LeftDelimiter,
        RightDelimiter = RightDelimiter
      };
    public override string DebugString =>
      new StringBuilder(HasRule ? @"\frac" : @"\atop")
        .AppendInBracketsOrNothing(LeftDelimiter.Nucleus)
        .AppendInBracketsOrNothing(RightDelimiter.Nucleus)
        .AppendInBracesOrEmptyBraces(Numerator?.DebugString)
        .AppendInBracesOrEmptyBraces(Denominator?.DebugString)
        .AppendDebugStringOfScripts(this).ToString();
    public override bool Equals(object obj) => obj is Fraction f && EqualsFraction(f);
    public bool EqualsFraction(Fraction other) =>
      EqualsAtom(other)
      && Numerator.NullCheckingStructuralEquality(other.Numerator)
      && Denominator.NullCheckingStructuralEquality(other.Denominator)
      && LeftDelimiter == other.LeftDelimiter
      && RightDelimiter == other.RightDelimiter;
    public override int GetHashCode() =>
      (base.GetHashCode(), Numerator, Denominator, LeftDelimiter, RightDelimiter).GetHashCode();
  }
}