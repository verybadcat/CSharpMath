using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class Fraction : MathAtom, IFraction {
    public IMathList Numerator { get; set; }
    public IMathList Denominator { get; set; }
    public string LeftDelimiter { get; set; }
    public string RightDelimiter { get; set; }

    public bool HasRule { get; private set; }

    public Fraction(bool hasRule = true): base(MathAtomType.Fraction, "") {

    }

    public override string StringValue {
      get {
        var builder = new StringBuilder();
        if (HasRule) {
          builder.Append(@"\atop");
        } else {
          builder.Append(@"\frac");
        }
        if (LeftDelimiter!=null || RightDelimiter!=null) {
          builder.Append($@"[{LeftDelimiter.NullToNull(NullHandling.EmptyContent)}][{RightDelimiter.NullToNull(NullHandling.EmptyContent)}]");
        }
        builder.AppendInBraces(Numerator.StringValue, NullHandling.EmptyString);
        builder.AppendInBraces(Denominator.StringValue, NullHandling.EmptyString);
        builder.AppendScripts(this);
        return builder.ToString();
      }
    }
    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
  => visitor.Visit(this, helper);

    public override bool Equals(object obj) {
      if (obj is Fraction) {
        return EqualsFraction((Fraction)obj);
      }
      return false;
    }

    public bool EqualsFraction(Fraction other) 
      => EqualsAtom(other)
        && Numerator == other.Numerator
        && Denominator == other.Denominator
        && LeftDelimiter == other.LeftDelimiter
        && RightDelimiter == other.RightDelimiter;
    

    public override int GetHashCode() =>
      base.GetHashCode()
      + 17 * Numerator?.GetHashCode() ?? 0
      + 19 * Denominator?.GetHashCode() ?? 0
      + 61 * LeftDelimiter?.GetHashCode() ?? 0
      + 101 * RightDelimiter?.GetHashCode() ?? 0;
  }
}
