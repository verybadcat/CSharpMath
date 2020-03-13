using System.Text;

namespace CSharpMath.Atoms.Atom {
  public class Fraction : MathAtom {
    public MathList? Numerator { get; set; }
    public MathList? Denominator { get; set; }
    public string? LeftDelimiter { get; set; }
    public string? RightDelimiter { get; set; }
    /// <summary>In this context, a "rule" is a fraction line.</summary>
    public bool HasRule { get; }
    public Fraction(bool hasRule = true) : base() => HasRule = hasRule;
    public override bool ScriptsAllowed => true;
    public new Fraction Clone(bool finalize) => (Fraction)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Fraction(HasRule) {
      Numerator = Numerator?.Clone(finalize),
      Denominator = Denominator?.Clone(finalize),
      LeftDelimiter = LeftDelimiter,
      RightDelimiter = RightDelimiter
    };
    public override string DebugString =>
      new StringBuilder(HasRule ? @"\frac" : @"\atop")
        .AppendInBracketsOrNothing(LeftDelimiter)
        .AppendInBracketsOrNothing(RightDelimiter)
        .AppendInBracesOrEmptyBraces(Numerator?.DebugString)
        .AppendInBracesOrEmptyBraces(Denominator?.DebugString)
        .AppendDebugStringOfScripts(this).ToString();
    public override bool Equals(object obj) =>
      obj is Fraction f ? EqualsFraction(f) : false;
    public bool EqualsFraction(Fraction other) =>
      EqualsAtom(other)
      && Numerator.NullCheckingStructuralEquality(other.Numerator)
      && Denominator.NullCheckingStructuralEquality(other.Denominator)
      && LeftDelimiter == other.LeftDelimiter
      && RightDelimiter == other.RightDelimiter;
    public override int GetHashCode() =>
      unchecked(
        base.GetHashCode()
        + 17 * Numerator?.GetHashCode() ?? 0
        + 19 * Denominator?.GetHashCode() ?? 0
        + 61 * LeftDelimiter?.GetHashCode() ?? 0
        + 101 * RightDelimiter?.GetHashCode() ?? 0);
  }
}