using System.Text;

namespace CSharpMath.Atom.Atoms {
  public enum FractionStyle { Auto, Display, Text }
  public enum FractionAlignment { Center, Left, Right }

  public sealed class Fraction : MathAtom, IMathListContainer {
    public MathList Numerator { get; }
    public MathList Denominator { get; }
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
      new[] { Numerator, Denominator };
    public Boundary LeftDelimiter { get; set; }
    public Boundary RightDelimiter { get; set; }
    /// <summary>In this context, a "rule" is a fraction line.</summary>
    public bool HasRule { get; }
    /// <summary>Explicit style override: \dfrac/\dbinom/\cfrac force display,
    /// \tfrac/\tbinom force text. Auto honors the surrounding style (Rule 15a).</summary>
    public FractionStyle StyleOverride { get; set; } = FractionStyle.Auto;
    /// <summary>True for \cfrac: operands are typeset in display style with struts
    /// and the whole fraction is wrapped in surrounding 3mu thin space.</summary>
    public bool IsContinuedFraction { get; set; }
    /// <summary>Numerator alignment within max(numWidth, denWidth). Only \cfrac[l]/[r] sets a non-default value.</summary>
    public FractionAlignment NumeratorAlignment { get; set; } = FractionAlignment.Center;
    public Fraction(MathList numerator, MathList denominator, bool hasRule = true) =>
      (Numerator, Denominator, HasRule) = (numerator, denominator, hasRule);
    public override bool ScriptsAllowed => true;
    public new Fraction Clone(bool finalize) => (Fraction)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Fraction(Numerator.Clone(finalize), Denominator.Clone(finalize), HasRule) {
        LeftDelimiter = LeftDelimiter,
        RightDelimiter = RightDelimiter,
        StyleOverride = StyleOverride,
        IsContinuedFraction = IsContinuedFraction,
        NumeratorAlignment = NumeratorAlignment
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
      && RightDelimiter == other.RightDelimiter
      && HasRule == other.HasRule
      && StyleOverride == other.StyleOverride
      && IsContinuedFraction == other.IsContinuedFraction
      && NumeratorAlignment == other.NumeratorAlignment;
    public override int GetHashCode() =>
      (base.GetHashCode(), Numerator, Denominator, LeftDelimiter, RightDelimiter,
        HasRule, (StyleOverride, IsContinuedFraction, NumeratorAlignment)).GetHashCode();
  }
}
